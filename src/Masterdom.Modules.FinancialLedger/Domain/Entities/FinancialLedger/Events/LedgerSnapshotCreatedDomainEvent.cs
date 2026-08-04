using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record LedgerSnapshotCreatedDomainEvent(
    LedgerId LedgerId,
    Guid SnapshotId,
    int VersionNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
