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
        ArgumentNullException.ThrowIfNull(operation);

        using var transaction = _dbContext.Database.BeginTransaction();

        operation();
        _dbContext.SaveChanges();

        transaction.Commit();
    }
}
