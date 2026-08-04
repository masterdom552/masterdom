using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

public sealed record PaymentReversedDomainEvent(
    PaymentId PaymentId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
