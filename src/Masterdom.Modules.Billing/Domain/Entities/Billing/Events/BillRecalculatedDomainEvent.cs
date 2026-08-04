using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing.Events;

public sealed record BillRecalculatedDomainEvent(
    BillId BillId,
    SnapshotVersion SnapshotVersion,
    DateTime OccurredOnUtc) : IDomainEvent;
