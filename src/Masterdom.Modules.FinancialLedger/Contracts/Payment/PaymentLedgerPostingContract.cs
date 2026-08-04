namespace Masterdom.Modules.FinancialLedger.Contracts.Payment;

public sealed record PaymentLedgerPostingContract(
    string PostingReference,
    string JournalNumber,
    DateOnly PostingDate,
    string Description,
    string BatchReference,
    IReadOnlyCollection<PaymentLedgerPostingLineContract> Lines);
