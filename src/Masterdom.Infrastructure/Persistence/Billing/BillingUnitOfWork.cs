using Masterdom.Modules.Billing.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Billing;

/// <summary>
/// EF Core unit-of-work implementation for billing application operations.
/// </summary>
public sealed class BillingUnitOfWork : IBillingUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public BillingUnitOfWork(MasterdomDbContext dbContext)
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
