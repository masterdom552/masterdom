using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Support;

public interface IMaintenancePlatformOrchestrator
{
    void OnMaintenanceTicketMutated(MaintenanceTicketAggregate maintenanceTicket, string operationName);
}
