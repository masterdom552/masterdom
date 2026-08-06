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

    public InventoryApplicationService(
        IInventoryItemRepository repository,
        IInventoryUnitOfWork unitOfWork,
        IInventoryPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public InventoryItemAggregate CreateInventoryItem(CreateInventoryItemCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = _repository.GetBySku(command.PropertyId, command.Sku);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Inventory item with SKU '{command.Sku.Trim()}' already exists for property '{command.PropertyId}'.");
        }

        var inventoryItem = InventoryItemAggregate.Create(
            InventoryItemId.New(),
            command.PropertyId,
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
}
