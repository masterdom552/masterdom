using Masterdom.Modules.Properties.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Property;

/// <summary>
/// EF Core unit-of-work implementation for property application operations.
/// </summary>
public sealed class PropertyUnitOfWork : IPropertyUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public PropertyUnitOfWork(MasterdomDbContext dbContext)
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
