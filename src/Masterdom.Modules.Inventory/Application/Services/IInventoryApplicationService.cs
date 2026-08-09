using Masterdom.Modules.Inventory.Application.Commands;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Modules.Inventory.Application.Services;

public interface IInventoryApplicationService
{
    InventoryItemAggregate CreateInventoryItem(CreateInventoryItemCommand command);

    InventoryItemAggregate ReceiveStock(ReceiveStockCommand command);

    InventoryItemAggregate AdjustStock(AdjustStockCommand command);

    InventoryItemAggregate TransferInventory(TransferInventoryCommand command);
}
