using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

public sealed record LedgerTransactionCreatedDomainEvent(
    LedgerId LedgerId,
    Guid LedgerTransactionId,
    string PostingReference,
    DateTime OccurredOnUtc) : IDomainEvent;
