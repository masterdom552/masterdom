using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

public sealed record ReceiptGeneratedDomainEvent(
    PaymentId PaymentId,
    string ReceiptNumber,
    int VersionNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
