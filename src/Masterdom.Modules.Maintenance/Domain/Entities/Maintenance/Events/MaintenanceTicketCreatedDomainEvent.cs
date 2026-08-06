using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.Events;

public sealed record MaintenanceTicketCreatedDomainEvent(
    MaintenanceTicketId MaintenanceTicketId,
    Guid PropertyId,
    Guid UnitId,
    DateTime OccurredOnUtc) : IDomainEvent;
