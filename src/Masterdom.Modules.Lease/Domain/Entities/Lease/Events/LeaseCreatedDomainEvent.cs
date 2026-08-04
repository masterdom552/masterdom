using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record LeaseCreatedDomainEvent(
    LeaseId LeaseId,
    LeaseNumber LeaseNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
