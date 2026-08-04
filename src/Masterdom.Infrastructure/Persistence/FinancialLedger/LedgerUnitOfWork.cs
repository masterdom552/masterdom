using Masterdom.Modules.FinancialLedger.Application.Support;

namespace Masterdom.Infrastructure.Persistence.FinancialLedger;

public sealed class LedgerUnitOfWork : ILedgerUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public LedgerUnitOfWork(MasterdomDbContext dbContext)
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
