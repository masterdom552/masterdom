using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

public sealed record InventoryItemCreatedDomainEvent(
    InventoryItemId InventoryItemId,
    Guid PropertyId,
    Guid StockLocationId,
    string Sku,
    DateTime OccurredOnUtc) : IDomainEvent;
