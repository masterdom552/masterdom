namespace Masterdom.Modules.Maintenance.Application.Commands;

public sealed record CreateMaintenanceTicketCommand(
    Guid PropertyId,
    Guid UnitId,
    string Title,
    string Description,
    DateTime CreatedAtUtc);
