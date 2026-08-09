using Masterdom.Modules.Inventory.Domain.Entities.Inventory;

namespace Masterdom.Modules.Inventory.Application.Commands;

public sealed record TransferInventoryCommand(
    InventoryItemId SourceInventoryItemId,
    Guid DestinationStockLocationId,
    decimal TransferQuantity);
