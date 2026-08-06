using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Modules.Inventory.Application.Support;

public interface IInventoryPlatformOrchestrator
{
    void OnInventoryItemMutated(InventoryItemAggregate inventoryItem, string operationName);
}
