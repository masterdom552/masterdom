using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

public sealed record PaymentReceivedDomainEvent(
    PaymentId PaymentId,
    string PaymentReference,
    decimal Amount,
    DateTime OccurredOnUtc) : IDomainEvent;
