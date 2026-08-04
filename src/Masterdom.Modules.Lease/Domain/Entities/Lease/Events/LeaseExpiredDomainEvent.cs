using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record LeaseExpiredDomainEvent(
    LeaseId LeaseId,
    DateTime OccurredOnUtc) : IDomainEvent;
