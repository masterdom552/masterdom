using Masterdom.Modules.Inventory.Domain.Entities.Inventory;

namespace Masterdom.Modules.Inventory.Application.Commands;

public sealed record AdjustStockCommand(
    InventoryItemId InventoryItemId,
    decimal AdjustmentQuantity);
