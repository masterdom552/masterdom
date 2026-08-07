using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Queries;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Modules.Maintenance.Application.Services;

public interface IMaintenanceApplicationService
{
    MaintenanceTicketAggregate CreateMaintenanceTicket(CreateMaintenanceTicketCommand command);

    MaintenanceTicketAggregate AssignMaintenanceTicket(AssignMaintenanceTicketCommand command);

    MaintenanceTicketAggregate CloseMaintenanceTicket(CloseMaintenanceTicketCommand command);

    MaintenanceTicketAggregate? GetMaintenanceTicketById(GetMaintenanceTicketByIdQuery query);
}
