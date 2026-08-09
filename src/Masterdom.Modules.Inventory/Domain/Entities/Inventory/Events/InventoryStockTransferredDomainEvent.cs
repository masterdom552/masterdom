using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

public sealed record InventoryStockTransferredDomainEvent(
    Guid PropertyId,
    string Sku,
    Guid SourceStockLocationId,
    Guid DestinationStockLocationId,
    decimal Quantity,
    DateTime OccurredOnUtc) : IDomainEvent;
