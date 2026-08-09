using Masterdom.Modules.CRM.Application.Support;

namespace Masterdom.Infrastructure.Persistence.CRM;

/// <summary>
/// EF Core unit-of-work implementation for CRM party application operations.
/// </summary>
public sealed class PartyUnitOfWork : IPartyUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public PartyUnitOfWork(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Execute(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        DbContextUnitOfWorkExecutor.Execute(_dbContext, operation);
    }
}
