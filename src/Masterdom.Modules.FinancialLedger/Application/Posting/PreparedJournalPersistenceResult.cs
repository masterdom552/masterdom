namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PreparedJournalPersistenceResult
{
    public PreparedJournalPersistenceResult(
        Guid persistenceId,
        Guid? ledgerTransactionId,
        string postingReference,
        string journalNumber,
        JournalLifecycleState state,
        bool wasIdempotentReplay)
    {
        if (persistenceId == Guid.Empty)
        {
            throw new ArgumentException("Persistence identifier cannot be empty.", nameof(persistenceId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(postingReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(journalNumber);

        PersistenceId = persistenceId;
        LedgerTransactionId = ledgerTransactionId;
        PostingReference = postingReference.Trim();
        JournalNumber = journalNumber.Trim();
        State = state;
        WasIdempotentReplay = wasIdempotentReplay;
    }

    public Guid PersistenceId { get; }

    public Guid? LedgerTransactionId { get; }

    public string PostingReference { get; }

    public string JournalNumber { get; }

    public JournalLifecycleState State { get; }

    public bool WasIdempotentReplay { get; }
}
