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
        DbContextUnitOfWorkExecutor.Execute(_dbContext, operation);
    }
}
