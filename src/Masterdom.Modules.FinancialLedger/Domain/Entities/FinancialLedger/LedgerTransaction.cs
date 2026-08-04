namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class LedgerTransaction
{
    private readonly List<JournalEntry> _journalEntries = [];

    private LedgerTransaction(
        Guid transactionId,
        PostingReference postingReference,
        string sourceModule,
        PostingDate postingDate,
        string journalNumber,
        string description,
        PostingStatus postingStatus,
        bool isReversal,
        Guid? reversedTransactionId,
        DateTime createdAtUtc)
    {
        TransactionId = transactionId;
        PostingReference = postingReference;
        SourceModule = sourceModule;
        PostingDate = postingDate;
        JournalNumber = journalNumber;
        Description = description;
        PostingStatus = postingStatus;
        IsReversal = isReversal;
        ReversedTransactionId = reversedTransactionId;
        CreatedAtUtc = createdAtUtc;
    }

    private LedgerTransaction(
        Guid transactionId,
        PostingReference postingReference,
        string sourceModule,
        PostingDate postingDate,
        Journal journal,
        PostingStatus postingStatus,
        bool isReversal,
        Guid? reversedTransactionId,
        DateTime createdAtUtc)
        : this(
            transactionId,
            postingReference,
            sourceModule,
            postingDate,
            journal.JournalNumber,
            journal.Description,
            postingStatus,
            isReversal,
            reversedTransactionId,
            createdAtUtc)
    {
        _journalEntries.AddRange(journal.Entries);
    }

    public Guid TransactionId { get; private set; }

    public PostingReference PostingReference { get; private set; }

    public string SourceModule { get; private set; }

    public PostingDate PostingDate { get; private set; }

    public string JournalNumber { get; private set; }

    public string Description { get; private set; }

    public PostingStatus PostingStatus { get; private set; }

    public bool IsReversal { get; private set; }

    public Guid? ReversedTransactionId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<JournalEntry> JournalEntries => _journalEntries.AsReadOnly();

    public decimal DebitTotal => _journalEntries.Sum(x => x.DebitAmount.Value);

    public decimal CreditTotal => _journalEntries.Sum(x => x.CreditAmount.Value);

    public static LedgerTransaction Create(
        PostingReference postingReference,
        string sourceModule,
        PostingDate postingDate,
        Journal journal,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(postingReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);
        ArgumentNullException.ThrowIfNull(postingDate);
        ArgumentNullException.ThrowIfNull(journal);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger transaction timestamp must be UTC.");
        }

        return new LedgerTransaction(
            Guid.CreateVersion7(),
            postingReference,
            sourceModule.Trim(),
            postingDate,
            journal,
            PostingStatus.Posted,
            false,
            null,
            createdAtUtc);
    }

    public LedgerTransaction Reverse(string reversalJournalNumber, string reason, DateTime reversedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reversalJournalNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (reversedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger transaction reversal timestamp must be UTC.");
        }

        var reversedEntries = _journalEntries
            .Select(x => JournalEntry.Create(
                x.AccountReference,
                MoneyAmount.Create(x.CreditAmount.Value),
                MoneyAmount.Create(x.DebitAmount.Value),
                $"Reversal: {reason.Trim()}"))
            .ToList();

        var reversalJournal = Journal.Create(reversalJournalNumber, $"Reversal of {JournalNumber}", reversedEntries);

        return new LedgerTransaction(
            Guid.CreateVersion7(),
            PostingReference,
            SourceModule,
            PostingDate,
            reversalJournal,
            PostingStatus.Reversed,
            true,
            TransactionId,
            reversedAtUtc);
    }
}
