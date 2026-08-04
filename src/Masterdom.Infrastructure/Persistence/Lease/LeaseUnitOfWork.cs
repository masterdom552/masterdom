using Masterdom.Modules.Lease.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Lease;

/// <summary>
/// EF Core unit-of-work implementation for lease application operations.
/// </summary>
public sealed class LeaseUnitOfWork : ILeaseUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public LeaseUnitOfWork(MasterdomDbContext dbContext)
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
