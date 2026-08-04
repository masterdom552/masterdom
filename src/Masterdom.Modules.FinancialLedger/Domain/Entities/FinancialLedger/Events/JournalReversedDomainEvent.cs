using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record JournalReversedDomainEvent(
    LedgerId LedgerId,
    string JournalNumber,
    Guid ReversedTransactionId,
    DateTime OccurredOnUtc) : IDomainEvent;
