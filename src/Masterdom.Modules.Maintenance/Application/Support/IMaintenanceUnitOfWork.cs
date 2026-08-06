namespace Masterdom.Modules.Maintenance.Application.Support;

public interface IMaintenanceUnitOfWork
{
    void Execute(Action operation);
}
