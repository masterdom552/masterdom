using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory;

public sealed class InventoryItem : AggregateRoot<InventoryItemId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private InventoryItem(
        InventoryItemId id,
        Guid propertyId,
        Guid stockLocationId,
        string sku,
        string name,
        decimal quantityOnHand,
        DateTime createdAtUtc)
        : base(id)
    {
        PropertyId = propertyId;
        StockLocationId = stockLocationId;
        Sku = sku;
        Name = name;
        QuantityOnHand = quantityOnHand;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid PropertyId { get; private set; }

    public Guid StockLocationId { get; private set; }

    public string Sku { get; private set; }

    public string Name { get; private set; }

    public decimal QuantityOnHand { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static InventoryItem Create(
        InventoryItemId id,
        Guid propertyId,
        Guid stockLocationId,
        string sku,
        string name,
        decimal quantityOnHand,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("PropertyId cannot be empty.", nameof(propertyId));
        }

        if (stockLocationId == Guid.Empty)
        {
            throw new ArgumentException("StockLocationId cannot be empty.", nameof(stockLocationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (sku.Length > 64)
        {
            throw new ArgumentException("Sku cannot exceed 64 characters.", nameof(sku));
        }

        if (name.Length > 200)
        {
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));
        }

        if (quantityOnHand < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityOnHand), "QuantityOnHand cannot be negative.");
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("CreatedAtUtc must be in UTC.");
        }

        var inventoryItem = new InventoryItem(
            id,
            propertyId,
            stockLocationId,
            sku.Trim(),
            name.Trim(),
            quantityOnHand,
            createdAtUtc);

        inventoryItem.Raise(new InventoryItemCreatedDomainEvent(
            inventoryItem.Id,
            inventoryItem.PropertyId,
            inventoryItem.StockLocationId,
            inventoryItem.Sku,
            inventoryItem.CreatedAtUtc));

        return inventoryItem;
    }

    public void ReceiveStock(decimal receivedQuantity)
    {
        if (receivedQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(receivedQuantity), "ReceivedQuantity must be greater than zero.");
        }

        QuantityOnHand += receivedQuantity;

        Raise(new InventoryStockReceivedDomainEvent(
            Id,
            PropertyId,
            receivedQuantity,
            DateTime.UtcNow));
    }

    public void AdjustStock(decimal adjustmentQuantity)
    {
        if (adjustmentQuantity == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustmentQuantity), "AdjustmentQuantity cannot be zero.");
        }

        var adjustedQuantityOnHand = QuantityOnHand + adjustmentQuantity;
        if (adjustedQuantityOnHand < 0)
        {
            throw new InvalidOperationException("QuantityOnHand cannot be negative.");
        }

        QuantityOnHand = adjustedQuantityOnHand;

        Raise(new InventoryStockAdjustedDomainEvent(
            Id,
            PropertyId,
            adjustmentQuantity,
            QuantityOnHand,
            DateTime.UtcNow));
    }

    public void TransferStockTo(InventoryItem destinationInventoryItem, decimal transferQuantity)
    {
        ArgumentNullException.ThrowIfNull(destinationInventoryItem);

        if (destinationInventoryItem.PropertyId != PropertyId)
        {
            throw new InvalidOperationException("Source and destination inventory items must belong to the same property.");
        }

        if (destinationInventoryItem.StockLocationId == StockLocationId)
        {
            throw new InvalidOperationException("Source and destination stock locations must be different.");
        }

        if (destinationInventoryItem.Sku != Sku)
        {
            throw new InvalidOperationException("Source and destination inventory items must have the same SKU.");
        }

        if (transferQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(transferQuantity), "TransferQuantity must be greater than zero.");
        }

        if (QuantityOnHand < transferQuantity)
        {
            throw new InvalidOperationException("Insufficient stock quantity for transfer.");
        }

        QuantityOnHand -= transferQuantity;
        destinationInventoryItem.QuantityOnHand += transferQuantity;

        Raise(new InventoryStockTransferredDomainEvent(
            PropertyId,
            Sku,
            StockLocationId,
            destinationInventoryItem.StockLocationId,
            transferQuantity,
            DateTime.UtcNow));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
