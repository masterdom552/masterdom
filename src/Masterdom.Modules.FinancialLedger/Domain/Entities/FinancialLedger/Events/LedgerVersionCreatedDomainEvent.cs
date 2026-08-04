using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record LedgerVersionCreatedDomainEvent(
    LedgerId LedgerId,
    int VersionNumber,
    string ChangeReason,
    DateTime OccurredOnUtc) : IDomainEvent;
