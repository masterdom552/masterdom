using Masterdom.Modules.Tenancy.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Tenancy;

/// <summary>
/// EF Core unit-of-work implementation for tenancy application operations.
/// </summary>
public sealed class TenancyUnitOfWork : ITenancyUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public TenancyUnitOfWork(MasterdomDbContext dbContext)
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
