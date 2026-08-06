using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

namespace Masterdom.Modules.Maintenance.Application.Commands;

public sealed record AssignMaintenanceTicketCommand(
    MaintenanceTicketId MaintenanceTicketId,
    Guid AssignedToPersonId,
    DateTime AssignedAtUtc);
