namespace Masterdom.Modules.Inventory.Application.Support;

public interface IInventoryStockLocationLookup
{
    (Guid PropertyId, bool IsActive)? Find(Guid stockLocationId);
}
