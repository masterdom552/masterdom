using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

public sealed record InventoryStockReceivedDomainEvent(
    InventoryItemId InventoryItemId,
    Guid PropertyId,
    decimal ReceivedQuantity,
    DateTime OccurredOnUtc) : IDomainEvent;
