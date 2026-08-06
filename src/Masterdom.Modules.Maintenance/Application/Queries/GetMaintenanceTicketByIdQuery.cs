using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

namespace Masterdom.Modules.Maintenance.Application.Queries;

public sealed record GetMaintenanceTicketByIdQuery(MaintenanceTicketId MaintenanceTicketId);
