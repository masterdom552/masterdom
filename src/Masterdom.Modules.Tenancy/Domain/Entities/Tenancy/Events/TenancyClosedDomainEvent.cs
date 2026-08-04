using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record TenancyClosedDomainEvent(
    TenancyId TenancyId,
    EffectiveDate ClosedOn,
    TerminationReason Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
