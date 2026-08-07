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
        DbContextUnitOfWorkExecutor.Execute(_dbContext, operation);
    }
}
