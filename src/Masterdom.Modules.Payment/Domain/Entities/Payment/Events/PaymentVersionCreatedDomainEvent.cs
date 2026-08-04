using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

public sealed record PaymentVersionCreatedDomainEvent(
    PaymentId PaymentId,
    int VersionNumber,
    string ChangeReason,
    DateTime OccurredOnUtc) : IDomainEvent;
