using Masterdom.Modules.Maintenance.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Maintenance;

public sealed class MaintenanceUnitOfWork : IMaintenanceUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public MaintenanceUnitOfWork(MasterdomDbContext dbContext)
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
