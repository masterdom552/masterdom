using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease.Events;

public sealed record CommercialTermsChangedDomainEvent(
    LeaseId LeaseId,
    int PreviousVersion,
    int NewVersion,
    DateTime OccurredOnUtc) : IDomainEvent;
