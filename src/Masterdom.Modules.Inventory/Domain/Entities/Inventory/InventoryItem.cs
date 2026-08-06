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
        string sku,
        string name,
        decimal quantityOnHand,
        DateTime createdAtUtc)
        : base(id)
    {
        PropertyId = propertyId;
        Sku = sku;
        Name = name;
        QuantityOnHand = quantityOnHand;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid PropertyId { get; private set; }

    public string Sku { get; private set; }

    public string Name { get; private set; }

    public decimal QuantityOnHand { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static InventoryItem Create(
        InventoryItemId id,
        Guid propertyId,
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
            sku.Trim(),
            name.Trim(),
            quantityOnHand,
            createdAtUtc);

        inventoryItem.Raise(new InventoryItemCreatedDomainEvent(
            inventoryItem.Id,
            inventoryItem.PropertyId,
            inventoryItem.Sku,
            inventoryItem.CreatedAtUtc));

        return inventoryItem;
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
