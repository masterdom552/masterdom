using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

namespace Masterdom.Core.Tests.Inventory;

public sealed class InventoryDomainTests
{
    [Fact]
    public void Create_ShouldCreateInventoryItemAndRaiseCreatedEvent()
    {
        var createdAtUtc = DateTime.UtcNow;

        var inventoryItem = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem.Create(
            InventoryItemId.New(),
            Guid.NewGuid(),
            "SKU-1001",
            "Air Filter",
            25m,
            createdAtUtc);

        Assert.Equal("SKU-1001", inventoryItem.Sku);
        Assert.Equal("Air Filter", inventoryItem.Name);
        Assert.Equal(25m, inventoryItem.QuantityOnHand);
        Assert.Contains(inventoryItem.DomainEvents, x => x is InventoryItemCreatedDomainEvent);
    }
}
