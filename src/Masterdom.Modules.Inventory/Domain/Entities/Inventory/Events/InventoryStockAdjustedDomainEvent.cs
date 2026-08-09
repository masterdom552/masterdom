using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

public sealed record InventoryStockAdjustedDomainEvent(
    InventoryItemId InventoryItemId,
    Guid PropertyId,
    decimal AdjustmentQuantity,
    decimal QuantityOnHand,
    DateTime OccurredOnUtc) : IDomainEvent;
