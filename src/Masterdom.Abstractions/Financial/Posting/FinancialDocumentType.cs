namespace Masterdom.Abstractions.Financial.Posting;

public enum FinancialDocumentType
{
    Unspecified = 0,
    Invoice = 1,
    Receipt = 2,
    CreditNote = 3,
    DebitNote = 4,
    JournalEntry = 5,
    Settlement = 6,
    Statement = 7,
    Other = 8
}
