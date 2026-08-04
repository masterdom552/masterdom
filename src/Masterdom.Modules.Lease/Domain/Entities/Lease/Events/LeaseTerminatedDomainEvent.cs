using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record LeaseTerminatedDomainEvent(
    LeaseId LeaseId,
    TerminationReason Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
