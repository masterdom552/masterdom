using Masterdom.Modules.Metering.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Metering;

public sealed class MeteringUnitOfWork : IMeteringUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public MeteringUnitOfWork(MasterdomDbContext dbContext)
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
