using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.Events;

public sealed record MaintenanceTicketAssignedDomainEvent(
    MaintenanceTicketId MaintenanceTicketId,
    Guid PropertyId,
    Guid AssignedToPersonId,
    DateTime OccurredOnUtc) : IDomainEvent;
