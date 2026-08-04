using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing.Events;

public sealed record BillVoidedDomainEvent(
    BillId BillId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
