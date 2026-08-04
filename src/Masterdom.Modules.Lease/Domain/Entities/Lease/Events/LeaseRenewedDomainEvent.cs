using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record LeaseRenewedDomainEvent(
    LeaseId LeaseId,
    int PreviousVersion,
    int NewVersion,
    RenewalDate RenewalDate,
    DateTime OccurredOnUtc) : IDomainEvent;
