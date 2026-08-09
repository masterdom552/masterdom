using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Repositories;

namespace Masterdom.Infrastructure.Persistence.Inventory;

public sealed class InventoryItemRepository : IInventoryItemRepository
{
    private readonly MasterdomDbContext _dbContext;

    public InventoryItemRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(InventoryItem inventoryItem)
    {
        ArgumentNullException.ThrowIfNull(inventoryItem);
        _dbContext.InventoryItems.Add(inventoryItem);
    }

    public void Update(InventoryItem inventoryItem)
    {
        ArgumentNullException.ThrowIfNull(inventoryItem);
        _dbContext.InventoryItems.Update(inventoryItem);
    }

    public InventoryItem? GetById(InventoryItemId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _dbContext.InventoryItems.FirstOrDefault(x => x.Id == id);
    }

    public InventoryItem? GetBySku(Guid propertyId, string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        var normalizedSku = sku.Trim();

        return _dbContext.InventoryItems
            .FirstOrDefault(x => x.PropertyId == propertyId && x.Sku == normalizedSku);
    }

    public InventoryItem? GetBySkuAndLocation(Guid propertyId, Guid stockLocationId, string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        var normalizedSku = sku.Trim();

        return _dbContext.InventoryItems
            .FirstOrDefault(x => x.PropertyId == propertyId && x.StockLocationId == stockLocationId && x.Sku == normalizedSku);
    }
}
