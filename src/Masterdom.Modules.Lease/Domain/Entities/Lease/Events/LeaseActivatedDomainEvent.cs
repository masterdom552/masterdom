using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record LeaseActivatedDomainEvent(
    LeaseId LeaseId,
    int VersionNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
