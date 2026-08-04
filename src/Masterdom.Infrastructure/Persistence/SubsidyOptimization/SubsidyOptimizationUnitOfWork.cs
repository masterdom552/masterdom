using Masterdom.Modules.SubsidyOptimization.Application.Support;

namespace Masterdom.Infrastructure.Persistence.SubsidyOptimization;

public sealed class SubsidyOptimizationUnitOfWork : ISubsidyOptimizationUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public SubsidyOptimizationUnitOfWork(MasterdomDbContext dbContext)
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
