using Masterdom.Modules.Inventory.Domain.Entities.Inventory;

namespace Masterdom.Modules.Inventory.Application.Commands;

public sealed record ReceiveStockCommand(
    InventoryItemId InventoryItemId,
    decimal ReceivedQuantity);
