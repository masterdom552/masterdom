using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

public sealed record InventoryItemCreatedDomainEvent(
    InventoryItemId InventoryItemId,
    Guid PropertyId,
    string Sku,
    DateTime OccurredOnUtc) : IDomainEvent;
