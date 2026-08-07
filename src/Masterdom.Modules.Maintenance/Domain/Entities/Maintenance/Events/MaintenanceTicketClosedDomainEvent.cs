using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.Events;

public sealed record MaintenanceTicketClosedDomainEvent(
    MaintenanceTicketId MaintenanceTicketId,
    Guid PropertyId,
    DateTime OccurredOnUtc) : IDomainEvent;
