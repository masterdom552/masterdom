using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Repositories;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Modules.Inventory.Application.Services;

public sealed class InventoryApplicationService : IInventoryApplicationService
{
    private readonly IInventoryItemRepository _repository;
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IInventoryPlatformOrchestrator _platformOrchestrator;
    private readonly IInventoryStockLocationLookup _stockLocationLookup;

    public InventoryApplicationService(
        IInventoryItemRepository repository,
        IInventoryUnitOfWork unitOfWork,
        IInventoryPlatformOrchestrator platformOrchestrator,
        IInventoryStockLocationLookup stockLocationLookup)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
        _stockLocationLookup = stockLocationLookup ?? throw new ArgumentNullException(nameof(stockLocationLookup));
    }

    public InventoryItemAggregate CreateInventoryItem(CreateInventoryItemCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = _repository.GetBySkuAndLocation(command.PropertyId, command.StockLocationId, command.Sku);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Inventory item with SKU '{command.Sku.Trim()}' already exists at location '{command.StockLocationId}' for property '{command.PropertyId}'.");
        }

        var inventoryItem = InventoryItemAggregate.Create(
            InventoryItemId.New(),
            command.PropertyId,
            command.StockLocationId,
            command.Sku,
            command.Name,
            command.QuantityOnHand,
            command.CreatedAtUtc);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(inventoryItem);
        });

        _platformOrchestrator.OnInventoryItemMutated(inventoryItem, "CreateInventoryItem");
        return inventoryItem;
    }

    public InventoryItemAggregate ReceiveStock(ReceiveStockCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var inventoryItem = _repository.GetById(command.InventoryItemId);
        if (inventoryItem is null)
        {
            throw new InvalidOperationException($"Inventory item '{command.InventoryItemId}' was not found.");
        }

        inventoryItem.ReceiveStock(command.ReceivedQuantity);

        _unitOfWork.Execute(() =>
        {
            _repository.Update(inventoryItem);
        });

        _platformOrchestrator.OnInventoryItemMutated(inventoryItem, "ReceiveStock");
        return inventoryItem;
    }

    public InventoryItemAggregate AdjustStock(AdjustStockCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var inventoryItem = _repository.GetById(command.InventoryItemId);
        if (inventoryItem is null)
        {
            throw new InvalidOperationException($"Inventory item '{command.InventoryItemId}' was not found.");
        }

        inventoryItem.AdjustStock(command.AdjustmentQuantity);

        _unitOfWork.Execute(() =>
        {
            _repository.Update(inventoryItem);
        });

        _platformOrchestrator.OnInventoryItemMutated(inventoryItem, "AdjustStock");
        return inventoryItem;
    }

    public InventoryItemAggregate TransferInventory(TransferInventoryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sourceInventoryItem = _repository.GetById(command.SourceInventoryItemId);
        if (sourceInventoryItem is null)
        {
            throw new InvalidOperationException($"Source inventory item '{command.SourceInventoryItemId}' was not found.");
        }

        var sourceLocation = _stockLocationLookup.Find(sourceInventoryItem.StockLocationId);
        if (sourceLocation is null || !sourceLocation.Value.IsActive)
        {
            throw new InvalidOperationException("Source stock location is not active.");
        }

        var destinationLocation = _stockLocationLookup.Find(command.DestinationStockLocationId);
        if (destinationLocation is null)
        {
            throw new InvalidOperationException($"Destination stock location '{command.DestinationStockLocationId}' was not found.");
        }

        if (destinationLocation.Value.PropertyId != sourceInventoryItem.PropertyId)
        {
            throw new InvalidOperationException("Destination stock location does not belong to the same property as the source inventory item.");
        }

        if (!destinationLocation.Value.IsActive)
        {
            throw new InvalidOperationException("Destination stock location is not active.");
        }

        var destinationInventoryItem = _repository.GetBySkuAndLocation(
            sourceInventoryItem.PropertyId,
            command.DestinationStockLocationId,
            sourceInventoryItem.Sku);

        if (destinationInventoryItem is null)
        {
            throw new InvalidOperationException(
                $"Destination inventory item for SKU '{sourceInventoryItem.Sku}' at location '{command.DestinationStockLocationId}' was not found.");
        }

        sourceInventoryItem.TransferStockTo(destinationInventoryItem, command.TransferQuantity);

        _unitOfWork.Execute(() =>
        {
            _repository.Update(sourceInventoryItem);
            _repository.Update(destinationInventoryItem);
        });

        _platformOrchestrator.OnInventoryItemMutated(sourceInventoryItem, "TransferInventoryOut");
        _platformOrchestrator.OnInventoryItemMutated(destinationInventoryItem, "TransferInventoryIn");

        return sourceInventoryItem;
    }
}

