using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory.Events;

namespace Masterdom.Core.Tests.Inventory;

public sealed class InventoryDomainTests
{
    [Fact]
    public void Create_ShouldCreateInventoryItemAndRaiseCreatedEvent()
    {
        var propertyId = Guid.NewGuid();
        var stockLocationId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var inventoryItem = InventoryItem.Create(
            InventoryItemId.New(),
            propertyId,
            stockLocationId,
            "SKU-1001",
            "Air Filter",
            25m,
            createdAtUtc);

        Assert.Equal("SKU-1001", inventoryItem.Sku);
        Assert.Equal("Air Filter", inventoryItem.Name);
        Assert.Equal(25m, inventoryItem.QuantityOnHand);
        Assert.Equal(propertyId, inventoryItem.PropertyId);
        Assert.Equal(stockLocationId, inventoryItem.StockLocationId);

        var ev = Assert.IsType<InventoryItemCreatedDomainEvent>(Assert.Single(inventoryItem.DomainEvents));
        Assert.Equal(stockLocationId, ev.StockLocationId);
    }

    [Fact]
    public void Create_WithEmptyStockLocationId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            InventoryItem.Create(InventoryItemId.New(), Guid.NewGuid(), Guid.Empty, "SKU-X", "Name", 0m, DateTime.UtcNow));
    }

    [Fact]
    public void Create_WithEmptyPropertyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            InventoryItem.Create(InventoryItemId.New(), Guid.Empty, Guid.NewGuid(), "SKU-X", "Name", 0m, DateTime.UtcNow));
    }

    [Fact]
    public void SameSku_AtDifferentLocations_ProducesDistinctItems()
    {
        var propertyId = Guid.NewGuid();
        var locationA = Guid.NewGuid();
        var locationB = Guid.NewGuid();

        var itemA = CreateInventoryItem(propertyId, locationA, "SKU-SHARED", 10m);
        var itemB = CreateInventoryItem(propertyId, locationB, "SKU-SHARED", 5m);

        Assert.NotEqual(itemA.Id, itemB.Id);
        Assert.Equal(itemA.Sku, itemB.Sku);
        Assert.Equal(itemA.PropertyId, itemB.PropertyId);
        Assert.NotEqual(itemA.StockLocationId, itemB.StockLocationId);
    }

    [Fact]
    public void ReceiveStock_WithPositiveQuantity_ShouldIncreaseQuantityAndRaiseReceivedEvent()
    {
        var inventoryItem = CreateInventoryItem(25m);
        inventoryItem.ClearDomainEvents();

        inventoryItem.ReceiveStock(2.5m);

        Assert.Equal(27.5m, inventoryItem.QuantityOnHand);
        var receivedEvent = Assert.IsType<InventoryStockReceivedDomainEvent>(
            Assert.Single(inventoryItem.DomainEvents));
        Assert.Equal(inventoryItem.Id, receivedEvent.InventoryItemId);
        Assert.Equal(inventoryItem.PropertyId, receivedEvent.PropertyId);
        Assert.Equal(2.5m, receivedEvent.ReceivedQuantity);
        Assert.Equal(DateTimeKind.Utc, receivedEvent.OccurredOnUtc.Kind);
    }

    [Fact]
    public void ReceiveStock_WithZeroQuantity_ShouldThrow()
    {
        var inventoryItem = CreateInventoryItem(25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => inventoryItem.ReceiveStock(0m));
        Assert.Equal(25m, inventoryItem.QuantityOnHand);
    }

    [Fact]
    public void ReceiveStock_WithNegativeQuantity_ShouldThrow()
    {
        var inventoryItem = CreateInventoryItem(25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => inventoryItem.ReceiveStock(-1m));
        Assert.Equal(25m, inventoryItem.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WithPositiveAdjustment_ShouldIncreaseQuantityAndRaiseAdjustedEvent()
    {
        var inventoryItem = CreateInventoryItem(25m);
        inventoryItem.ClearDomainEvents();

        inventoryItem.AdjustStock(2.5m);

        Assert.Equal(27.5m, inventoryItem.QuantityOnHand);
        var adjustedEvent = Assert.IsType<InventoryStockAdjustedDomainEvent>(
            Assert.Single(inventoryItem.DomainEvents));
        Assert.Equal(inventoryItem.Id, adjustedEvent.InventoryItemId);
        Assert.Equal(inventoryItem.PropertyId, adjustedEvent.PropertyId);
        Assert.Equal(2.5m, adjustedEvent.AdjustmentQuantity);
        Assert.Equal(27.5m, adjustedEvent.QuantityOnHand);
        Assert.Equal(DateTimeKind.Utc, adjustedEvent.OccurredOnUtc.Kind);
    }

    [Fact]
    public void AdjustStock_WithNegativeAdjustment_ShouldDecreaseQuantityAndRaiseAdjustedEvent()
    {
        var inventoryItem = CreateInventoryItem(25m);
        inventoryItem.ClearDomainEvents();

        inventoryItem.AdjustStock(-2.5m);

        Assert.Equal(22.5m, inventoryItem.QuantityOnHand);
        var adjustedEvent = Assert.IsType<InventoryStockAdjustedDomainEvent>(
            Assert.Single(inventoryItem.DomainEvents));
        Assert.Equal(-2.5m, adjustedEvent.AdjustmentQuantity);
        Assert.Equal(22.5m, adjustedEvent.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WithZeroAdjustment_ShouldThrow()
    {
        var inventoryItem = CreateInventoryItem(25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => inventoryItem.AdjustStock(0m));
        Assert.Equal(25m, inventoryItem.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WithAdjustmentCausingNegativeQuantity_ShouldThrowAndPreserveInvariant()
    {
        var inventoryItem = CreateInventoryItem(2m);

        Assert.Throws<InvalidOperationException>(() => inventoryItem.AdjustStock(-2.5m));
        Assert.Equal(2m, inventoryItem.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_WithAvailableQuantity_ShouldPreserveConservationAndRaiseEvent()
    {
        var propertyId = Guid.NewGuid();
        var sourceLocationId = Guid.NewGuid();
        var destLocationId = Guid.NewGuid();
        var sku = "SKU-TRANSFER";

        var source = CreateInventoryItem(propertyId, sourceLocationId, sku, 25m);
        var destination = CreateInventoryItem(propertyId, destLocationId, sku, 10m);

        source.ClearDomainEvents();
        destination.ClearDomainEvents();

        source.TransferStockTo(destination, 2.5m);

        Assert.Equal(22.5m, source.QuantityOnHand);
        Assert.Equal(12.5m, destination.QuantityOnHand);
        Assert.Equal(35m, source.QuantityOnHand + destination.QuantityOnHand);

        var ev = Assert.IsType<InventoryStockTransferredDomainEvent>(Assert.Single(source.DomainEvents));
        Assert.Equal(propertyId, ev.PropertyId);
        Assert.Equal(sku, ev.Sku);
        Assert.Equal(sourceLocationId, ev.SourceStockLocationId);
        Assert.Equal(destLocationId, ev.DestinationStockLocationId);
        Assert.Equal(2.5m, ev.Quantity);
        Assert.Equal(DateTimeKind.Utc, ev.OccurredOnUtc.Kind);

        Assert.Empty(destination.DomainEvents);
    }

    [Fact]
    public void TransferStockTo_SourceDecreases_DestinationIncreases()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-CONS";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 20m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 5m);

        source.TransferStockTo(dest, 8m);

        Assert.Equal(12m, source.QuantityOnHand);
        Assert.Equal(13m, dest.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_StockIsConserved()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-CONS2";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 100m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 50m);

        source.TransferStockTo(dest, 30m);

        Assert.Equal(150m, source.QuantityOnHand + dest.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_WithInsufficientQuantity_ShouldThrowAndPreserveInvariant()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-INSUF";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 2m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 5m);

        Assert.Throws<InvalidOperationException>(() => source.TransferStockTo(dest, 2.5m));
        Assert.Equal(2m, source.QuantityOnHand);
        Assert.Equal(5m, dest.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_WithZeroQuantity_ShouldThrow()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-ZERO";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 10m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 5m);

        Assert.Throws<ArgumentOutOfRangeException>(() => source.TransferStockTo(dest, 0m));
    }

    [Fact]
    public void TransferStockTo_WithNegativeQuantity_ShouldThrow()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-NEG";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 10m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 5m);

        Assert.Throws<ArgumentOutOfRangeException>(() => source.TransferStockTo(dest, -1m));
    }

    [Fact]
    public void TransferStockTo_WithSameLocation_ShouldThrow()
    {
        var propertyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var sku = "SKU-SAME";
        var source = CreateInventoryItem(propertyId, locationId, sku, 10m);
        var dest = CreateInventoryItem(propertyId, locationId, sku, 5m);

        Assert.Throws<InvalidOperationException>(() => source.TransferStockTo(dest, 1m));
        Assert.Equal(10m, source.QuantityOnHand);
        Assert.Equal(5m, dest.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_WithCrossPropertyDestination_ShouldThrow()
    {
        var sku = "SKU-CROSS";
        var source = CreateInventoryItem(Guid.NewGuid(), Guid.NewGuid(), sku, 10m);
        var dest = CreateInventoryItem(Guid.NewGuid(), Guid.NewGuid(), sku, 5m);

        Assert.Throws<InvalidOperationException>(() => source.TransferStockTo(dest, 1m));
        Assert.Equal(10m, source.QuantityOnHand);
        Assert.Equal(5m, dest.QuantityOnHand);
    }

    [Fact]
    public void TransferStockTo_WithDifferentSku_ShouldThrow()
    {
        var propertyId = Guid.NewGuid();
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), "SKU-A", 10m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), "SKU-B", 5m);

        Assert.Throws<InvalidOperationException>(() => source.TransferStockTo(dest, 1m));
    }

    [Fact]
    public void TransferStockTo_AtomicRollback_NeitherChangesOnThrow()
    {
        var propertyId = Guid.NewGuid();
        var sku = "SKU-ATOMIC";
        var source = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 2m);
        var dest = CreateInventoryItem(propertyId, Guid.NewGuid(), sku, 5m);

        Assert.Throws<InvalidOperationException>(() => source.TransferStockTo(dest, 10m));
        Assert.Equal(2m, source.QuantityOnHand);
        Assert.Equal(5m, dest.QuantityOnHand);
    }

    private static InventoryItem CreateInventoryItem(decimal quantityOnHand = 25m)
    {
        return InventoryItem.Create(
            InventoryItemId.New(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SKU-1001",
            "Air Filter",
            quantityOnHand,
            DateTime.UtcNow);
    }

    private static InventoryItem CreateInventoryItem(Guid propertyId, Guid stockLocationId, string sku, decimal quantityOnHand)
    {
        return InventoryItem.Create(
            InventoryItemId.New(),
            propertyId,
            stockLocationId,
            sku,
            $"Name for {sku}",
            quantityOnHand,
            DateTime.UtcNow);
    }
}
