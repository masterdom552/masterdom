using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment.Events;

public sealed record PaymentAllocatedDomainEvent(
    PaymentId PaymentId,
    decimal AllocatedAmount,
    int AllocationCount,
    DateTime OccurredOnUtc) : IDomainEvent;
