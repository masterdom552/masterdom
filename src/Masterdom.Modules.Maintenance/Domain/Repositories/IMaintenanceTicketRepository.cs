using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

namespace Masterdom.Modules.Maintenance.Domain.Repositories;

public interface IMaintenanceTicketRepository
{
    void Add(MaintenanceTicket maintenanceTicket);

    void Update(MaintenanceTicket maintenanceTicket);

    MaintenanceTicket? GetById(MaintenanceTicketId id);
}
