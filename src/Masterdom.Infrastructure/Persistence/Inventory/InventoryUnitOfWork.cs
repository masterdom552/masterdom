using Masterdom.Modules.Inventory.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Inventory;

public sealed class InventoryUnitOfWork : IInventoryUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public InventoryUnitOfWork(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Execute(Action operation)
    {
        DbContextUnitOfWorkExecutor.Execute(_dbContext, operation);
    }
}
