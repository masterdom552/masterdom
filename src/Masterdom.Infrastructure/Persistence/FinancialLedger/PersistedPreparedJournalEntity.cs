namespace Masterdom.Infrastructure.Persistence.FinancialLedger;

public sealed class PersistedPreparedJournalEntity
{
    public Guid Id { get; set; }

    public Guid LedgerId { get; set; }

    public string PostingReference { get; set; } = string.Empty;

    public string JournalReference { get; set; } = string.Empty;

    public string JournalNumber { get; set; } = string.Empty;

    public DateOnly PostingDate { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string BatchReference { get; set; } = string.Empty;

    public string SourceModule { get; set; } = string.Empty;

    public Guid BillId { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public decimal DebitTotal { get; set; }

    public decimal CreditTotal { get; set; }

    public string State { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ValidatedAtUtc { get; set; }

    public DateTime? PostedAtUtc { get; set; }

    public DateTime? ReversedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public string? CancellationReason { get; set; }

    public Guid? LedgerTransactionId { get; set; }

    public string LinesJson { get; set; } = "[]";

    public string MetadataJson { get; set; } = "{}";
}
