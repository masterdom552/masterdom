using Masterdom.Modules.PolicyFramework.Application.Support;

namespace Masterdom.Infrastructure.Persistence.PolicyFramework;

public sealed class PolicyFrameworkUnitOfWork : IPolicyFrameworkUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public PolicyFrameworkUnitOfWork(MasterdomDbContext dbContext)
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
