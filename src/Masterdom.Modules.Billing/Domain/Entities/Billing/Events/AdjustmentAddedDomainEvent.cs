using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing.Events;

public sealed record AdjustmentAddedDomainEvent(
    BillId BillId,
    SnapshotVersion SnapshotVersion,
    decimal Amount,
    DateTime OccurredOnUtc) : IDomainEvent;
