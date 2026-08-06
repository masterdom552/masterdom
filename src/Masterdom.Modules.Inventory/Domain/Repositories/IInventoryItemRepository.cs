using Masterdom.Modules.Inventory.Domain.Entities.Inventory;

namespace Masterdom.Modules.Inventory.Domain.Repositories;

public interface IInventoryItemRepository
{
    void Add(InventoryItem inventoryItem);

    InventoryItem? GetBySku(Guid propertyId, string sku);
}
