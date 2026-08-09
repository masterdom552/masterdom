using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Handlers.Commands;
using Masterdom.Modules.Inventory.Application.Services;
using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Repositories;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Core.Tests.Inventory;

public sealed class InventoryApplicationHandlerTests
{
    [Fact]
    public void Create_ShouldPersistInventoryItemAndInvokeOrchestrator()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        locationLookup.Add(locationId, propertyId);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new CreateInventoryItemCommandHandler(service);

        var result = handler.Handle(new CreateInventoryItemCommand(
            propertyId,
            locationId,
            "SKU-2200",
            "Safety Gloves",
            42m,
            DateTime.UtcNow));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
        Assert.Equal("SKU-2200", result.Value!.Sku);
        Assert.Equal(locationId, result.Value.StockLocationId);
    }

    [Fact]
    public void Create_DuplicateSkuAtSameLocation_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        locationLookup.Add(locationId, propertyId);

        var existing = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, locationId, "SKU-DUP", "Dup Item", 1m, DateTime.UtcNow);
        repository.Add(existing);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new CreateInventoryItemCommandHandler(service);

        var result = handler.Handle(new CreateInventoryItemCommand(propertyId, locationId, "SKU-DUP", "Another", 1m, DateTime.UtcNow));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void Create_SameSku_DifferentLocation_ShouldSucceed()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var locationA = Guid.NewGuid();
        var locationB = Guid.NewGuid();
        locationLookup.Add(locationA, propertyId);
        locationLookup.Add(locationB, propertyId);

        var existing = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, locationA, "SKU-MULTI", "Item A", 1m, DateTime.UtcNow);
        repository.Add(existing);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new CreateInventoryItemCommandHandler(service);

        var result = handler.Handle(new CreateInventoryItemCommand(propertyId, locationB, "SKU-MULTI", "Item B", 5m, DateTime.UtcNow));

        Assert.True(result.IsSuccess);
        Assert.Equal(locationB, result.Value!.StockLocationId);
    }

    [Fact]
    public void ReceiveStock_ShouldUpdateInventoryItemAndInvokeOrchestrator()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var inventoryItem = InventoryItemAggregate.Create(InventoryItemId.New(), Guid.NewGuid(), Guid.NewGuid(), "SKU-2200", "Safety Gloves", 42m, DateTime.UtcNow);
        repository.Add(inventoryItem);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new ReceiveStockCommandHandler(service);

        var result = handler.Handle(new ReceiveStockCommand(inventoryItem.Id, 2.5m));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(44.5m, result.Value!.QuantityOnHand);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void ReceiveStock_WithMissingInventoryItem_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new ReceiveStockCommandHandler(service);

        var result = handler.Handle(new ReceiveStockCommand(InventoryItemId.New(), 2.5m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public void AdjustStock_ShouldUpdateInventoryItemAndInvokeOrchestrator()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var inventoryItem = InventoryItemAggregate.Create(InventoryItemId.New(), Guid.NewGuid(), Guid.NewGuid(), "SKU-2200", "Safety Gloves", 42m, DateTime.UtcNow);
        repository.Add(inventoryItem);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new AdjustStockCommandHandler(service);

        var result = handler.Handle(new AdjustStockCommand(inventoryItem.Id, -2.5m));

        Assert.True(result.IsSuccess);
        Assert.Equal(39.5m, result.Value!.QuantityOnHand);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public void AdjustStock_WithMissingInventoryItem_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new AdjustStockCommandHandler(service);

        var result = handler.Handle(new AdjustStockCommand(InventoryItemId.New(), -2.5m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public void TransferInventory_ShouldUpdateBothItemsAndInvokeOrchestrator()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, propertyId, isActive: true);
        locationLookup.Add(dstLocId, propertyId, isActive: true);

        var sku = "SKU-TRANSFER";
        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, srcLocId, sku, "Source Item", 42m, DateTime.UtcNow);
        var destination = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, dstLocId, sku, "Dest Item", 10m, DateTime.UtcNow);
        repository.Add(source);
        repository.Add(destination);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 2.5m));

        Assert.True(result.IsSuccess);
        Assert.Equal(39.5m, source.QuantityOnHand);
        Assert.Equal(12.5m, destination.QuantityOnHand);
        Assert.Equal(52m, source.QuantityOnHand + destination.QuantityOnHand);
        Assert.Equal(2, repository.UpdateCount);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(2, orchestrator.MutationCount);
    }

    [Fact]
    public void TransferInventory_WithInsufficientStock_ShouldFailAndPreserveQuantities()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, propertyId, isActive: true);
        locationLookup.Add(dstLocId, propertyId, isActive: true);

        var sku = "SKU-INSUF";
        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, srcLocId, sku, "Source", 2m, DateTime.UtcNow);
        var dest = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, dstLocId, sku, "Dest", 10m, DateTime.UtcNow);
        repository.Add(source);
        repository.Add(dest);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 2.5m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
        Assert.Equal(2m, source.QuantityOnHand);
        Assert.Equal(10m, dest.QuantityOnHand);
        Assert.Equal(0, repository.UpdateCount);
        Assert.Equal(0, unitOfWork.ExecuteCount);
    }

    [Fact]
    public void TransferInventory_WithMissingDestinationItem_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, propertyId, isActive: true);
        locationLookup.Add(dstLocId, propertyId, isActive: true);

        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, srcLocId, "SKU-X", "Source", 42m, DateTime.UtcNow);
        repository.Add(source);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 2.5m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
        Assert.Equal(42m, source.QuantityOnHand);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public void TransferInventory_WithInactiveSourceLocation_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, propertyId, isActive: false);
        locationLookup.Add(dstLocId, propertyId, isActive: true);

        var sku = "SKU-INACT-SRC";
        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, srcLocId, sku, "Source", 10m, DateTime.UtcNow);
        var dest = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, dstLocId, sku, "Dest", 5m, DateTime.UtcNow);
        repository.Add(source);
        repository.Add(dest);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 1m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void TransferInventory_WithInactiveDestinationLocation_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, propertyId, isActive: true);
        locationLookup.Add(dstLocId, propertyId, isActive: false);

        var sku = "SKU-INACT-DST";
        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, srcLocId, sku, "Source", 10m, DateTime.UtcNow);
        var dest = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, dstLocId, sku, "Dest", 5m, DateTime.UtcNow);
        repository.Add(source);
        repository.Add(dest);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 1m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void TransferInventory_WithCrossPropertyDestinationLocation_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var srcPropertyId = Guid.NewGuid();
        var dstPropertyId = Guid.NewGuid();
        var srcLocId = Guid.NewGuid();
        var dstLocId = Guid.NewGuid();
        locationLookup.Add(srcLocId, srcPropertyId, isActive: true);
        locationLookup.Add(dstLocId, dstPropertyId, isActive: true);

        var source = InventoryItemAggregate.Create(InventoryItemId.New(), srcPropertyId, srcLocId, "SKU-CROSS", "Source", 10m, DateTime.UtcNow);
        repository.Add(source);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, dstLocId, 1m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    [Fact]
    public void TransferInventory_WithSameSourceAndDestinationLocation_ShouldFail()
    {
        var (repository, unitOfWork, orchestrator, locationLookup) = BuildSpies();
        var propertyId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        locationLookup.Add(locationId, propertyId, isActive: true);

        var sku = "SKU-SAME-LOC";
        var source = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, locationId, sku, "Source", 10m, DateTime.UtcNow);
        var dest = InventoryItemAggregate.Create(InventoryItemId.New(), propertyId, locationId, sku, "Dest", 5m, DateTime.UtcNow);
        repository.Add(source);
        repository.Add(dest);

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator, locationLookup);
        var handler = new TransferInventoryCommandHandler(service);

        var result = handler.Handle(new TransferInventoryCommand(source.Id, locationId, 1m));

        Assert.False(result.IsSuccess);
        Assert.Equal("conflict", result.ErrorCode);
    }

    private static (InMemoryInventoryItemRepository, SpyUnitOfWork, SpyPlatformOrchestrator, InMemoryStockLocationLookup) BuildSpies()
    {
        return (new InMemoryInventoryItemRepository(), new SpyUnitOfWork(), new SpyPlatformOrchestrator(), new InMemoryStockLocationLookup());
    }

    private sealed class InMemoryInventoryItemRepository : IInventoryItemRepository
    {
        private readonly List<InventoryItemAggregate> _inventoryItems = [];

        public int UpdateCount { get; private set; }

        public void Add(InventoryItemAggregate inventoryItem) => _inventoryItems.Add(inventoryItem);

        public void Update(InventoryItemAggregate inventoryItem) => UpdateCount++;

        public InventoryItemAggregate? GetById(InventoryItemId id)
            => _inventoryItems.FirstOrDefault(x => x.Id == id);

        public InventoryItemAggregate? GetBySku(Guid propertyId, string sku)
            => _inventoryItems.FirstOrDefault(x => x.PropertyId == propertyId && string.Equals(x.Sku, sku, StringComparison.Ordinal));

        public InventoryItemAggregate? GetBySkuAndLocation(Guid propertyId, Guid stockLocationId, string sku)
            => _inventoryItems.FirstOrDefault(x => x.PropertyId == propertyId && x.StockLocationId == stockLocationId && string.Equals(x.Sku, sku, StringComparison.Ordinal));
    }

    private sealed class InMemoryStockLocationLookup : IInventoryStockLocationLookup
    {
        private readonly Dictionary<Guid, (Guid PropertyId, bool IsActive)> _locations = [];

        public void Add(Guid stockLocationId, Guid propertyId, bool isActive = true)
            => _locations[stockLocationId] = (propertyId, isActive);

        public (Guid PropertyId, bool IsActive)? Find(Guid stockLocationId)
            => _locations.TryGetValue(stockLocationId, out var info) ? info : null;
    }

    private sealed class SpyUnitOfWork : IInventoryUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IInventoryPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnInventoryItemMutated(InventoryItemAggregate inventoryItem, string operationName)
            => MutationCount++;
    }
}
