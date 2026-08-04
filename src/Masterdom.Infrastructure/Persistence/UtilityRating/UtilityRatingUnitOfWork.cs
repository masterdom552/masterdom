using Masterdom.Modules.UtilityRating.Application.Support;

namespace Masterdom.Infrastructure.Persistence.UtilityRating;

public sealed class UtilityRatingUnitOfWork : IUtilityRatingUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public UtilityRatingUnitOfWork(MasterdomDbContext dbContext)
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
