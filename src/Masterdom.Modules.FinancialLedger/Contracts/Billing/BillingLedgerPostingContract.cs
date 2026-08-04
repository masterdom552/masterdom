namespace Masterdom.Modules.FinancialLedger.Contracts.Billing;

public sealed record BillingLedgerPostingContract(
    string PostingReference,
    string JournalNumber,
    DateOnly PostingDate,
    string Description,
    string BatchReference,
    IReadOnlyCollection<LedgerPostingLineContract> Lines);
