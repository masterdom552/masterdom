using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Modules.Security.Infrastructure;

public sealed class IdentityAdministrationUnitOfWork : IIdentityAdministrationUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public IdentityAdministrationUnitOfWork(MasterdomDbContext dbContext)
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
