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
        var repository = new InMemoryInventoryItemRepository();
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new InventoryApplicationService(repository, unitOfWork, orchestrator);
        var handler = new CreateInventoryItemCommandHandler(service);

        var result = handler.Handle(new CreateInventoryItemCommand(
            Guid.NewGuid(),
            "SKU-2200",
            "Safety Gloves",
            42m,
            DateTime.UtcNow));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
        Assert.Equal("SKU-2200", result.Value!.Sku);
    }

    private sealed class InMemoryInventoryItemRepository : IInventoryItemRepository
    {
        private readonly List<InventoryItemAggregate> _inventoryItems = [];

        public void Add(InventoryItemAggregate inventoryItem)
        {
            _inventoryItems.Add(inventoryItem);
        }

        public InventoryItemAggregate? GetBySku(Guid propertyId, string sku)
        {
            return _inventoryItems.FirstOrDefault(
                x => x.PropertyId == propertyId && string.Equals(x.Sku, sku, StringComparison.Ordinal));
        }
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
        {
            MutationCount++;
        }
    }
}
