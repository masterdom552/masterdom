using Masterdom.Modules.Inventory.Domain.Entities.Inventory;

namespace Masterdom.Modules.Inventory.Domain.Repositories;

public interface IInventoryItemRepository
{
    void Add(InventoryItem inventoryItem);

    void Update(InventoryItem inventoryItem);

    InventoryItem? GetById(InventoryItemId id);

    InventoryItem? GetBySku(Guid propertyId, string sku);

    InventoryItem? GetBySkuAndLocation(Guid propertyId, Guid stockLocationId, string sku);
}
