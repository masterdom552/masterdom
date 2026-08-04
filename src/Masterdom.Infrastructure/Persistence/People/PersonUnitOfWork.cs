using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Infrastructure.Persistence.People;

/// <summary>
/// EF Core unit-of-work implementation for people application operations.
/// </summary>
public sealed class PersonUnitOfWork : IPersonUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public PersonUnitOfWork(MasterdomDbContext dbContext)
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
