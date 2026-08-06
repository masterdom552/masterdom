namespace Masterdom.Modules.Inventory.Application.Commands;

public sealed record CreateInventoryItemCommand(
    Guid PropertyId,
    string Sku,
    string Name,
    decimal QuantityOnHand,
    DateTime CreatedAtUtc);
