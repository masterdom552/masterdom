using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Inventory;

public sealed class InventoryStockLocationLookup : IInventoryStockLocationLookup
{
    private readonly MasterdomDbContext _dbContext;

    public InventoryStockLocationLookup(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public (Guid PropertyId, bool IsActive)? Find(Guid stockLocationId)
    {
        var id = new StockLocationId(stockLocationId);

        var location = _dbContext.StockLocations
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);

        if (location is null)
            return null;

        return (location.PropertyId.Value, location.IsActive);
    }
}
