using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

namespace Masterdom.Modules.Maintenance.Application.Commands;

public sealed record CloseMaintenanceTicketCommand(
    MaintenanceTicketId MaintenanceTicketId,
    DateTime ClosedAtUtc);
