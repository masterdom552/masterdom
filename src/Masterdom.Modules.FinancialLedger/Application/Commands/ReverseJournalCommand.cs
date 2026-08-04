using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Commands;

public sealed record ReverseJournalCommand(
    LedgerId LedgerId,
    Guid LedgerTransactionId,
    string ReversalJournalNumber,
    string Reason,
    DateTime ReversedAtUtc);
