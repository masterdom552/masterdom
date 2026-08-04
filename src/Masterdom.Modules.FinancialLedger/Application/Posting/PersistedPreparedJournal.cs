using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PersistedPreparedJournal
{
    private PersistedPreparedJournal(
        Guid persistenceId,
        LedgerId ledgerId,
        string postingReference,
        string journalReference,
        string journalNumber,
        DateOnly postingDate,
        string state,
        DateTime createdAtUtc,
        DateTime? validatedAtUtc,
        DateTime? postedAtUtc,
        DateTime? reversedAtUtc,
        DateTime? cancelledAtUtc,
        string? cancellationReason,
        Guid? ledgerTransactionId,
        PreparedJournal preparedJournal)
    {
        PersistenceId = persistenceId;
        LedgerId = ledgerId;
        PostingReference = postingReference;
        JournalReference = journalReference;
        JournalNumber = journalNumber;
        PostingDate = postingDate;
        State = state;
        CreatedAtUtc = createdAtUtc;
        ValidatedAtUtc = validatedAtUtc;
        PostedAtUtc = postedAtUtc;
        ReversedAtUtc = reversedAtUtc;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = cancellationReason;
        LedgerTransactionId = ledgerTransactionId;
        PreparedJournal = preparedJournal;
    }

    public Guid PersistenceId { get; }

    public LedgerId LedgerId { get; }

    public string PostingReference { get; }

    public string JournalReference { get; }

    public string JournalNumber { get; }

    public DateOnly PostingDate { get; }

    public string State { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime? ValidatedAtUtc { get; }

    public DateTime? PostedAtUtc { get; }

    public DateTime? ReversedAtUtc { get; }

    public DateTime? CancelledAtUtc { get; }

    public string? CancellationReason { get; }

    public Guid? LedgerTransactionId { get; }

    public PreparedJournal PreparedJournal { get; }

    public JournalLifecycleState LifecycleState => JournalLifecycleStateParser.Parse(State);

    public static PersistedPreparedJournal Create(LedgerId ledgerId, PreparedJournal preparedJournal, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(ledgerId);
        ArgumentNullException.ThrowIfNull(preparedJournal);
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        return new PersistedPreparedJournal(
            Guid.CreateVersion7(),
            ledgerId,
            preparedJournal.PostingReference,
            preparedJournal.JournalReference,
            preparedJournal.JournalNumber,
            preparedJournal.PostingDate,
            JournalLifecycleState.Prepared.ToString(),
            createdAtUtc,
            null,
            null,
            null,
            null,
            null,
            null,
            preparedJournal);
    }

    public PersistedPreparedJournal MarkValidated(DateTime validatedAtUtc)
    {
        var updatedJournal = PreparedJournal.MarkValidated(validatedAtUtc);

        return new PersistedPreparedJournal(
            PersistenceId,
            LedgerId,
            PostingReference,
            JournalReference,
            JournalNumber,
            PostingDate,
            JournalLifecycleState.Validated.ToString(),
            CreatedAtUtc,
            validatedAtUtc,
            PostedAtUtc,
            ReversedAtUtc,
            CancelledAtUtc,
            CancellationReason,
            LedgerTransactionId,
            updatedJournal);
    }

    public PersistedPreparedJournal MarkPosted(Guid ledgerTransactionId, DateTime postedAtUtc)
    {
        if (ledgerTransactionId == Guid.Empty)
        {
            throw new ArgumentException("Ledger transaction identifier cannot be empty.", nameof(ledgerTransactionId));
        }

        var updatedJournal = PreparedJournal.MarkPosted(postedAtUtc);

        return new PersistedPreparedJournal(
            PersistenceId,
            LedgerId,
            PostingReference,
            JournalReference,
            JournalNumber,
            PostingDate,
            JournalLifecycleState.Posted.ToString(),
            CreatedAtUtc,
            ValidatedAtUtc,
            postedAtUtc,
            ReversedAtUtc,
            CancelledAtUtc,
            CancellationReason,
            ledgerTransactionId,
            updatedJournal);
    }

    public PersistedPreparedJournal MarkCancelled(DateTime cancelledAtUtc, string reason)
    {
        var updatedJournal = PreparedJournal.MarkCancelled(cancelledAtUtc, reason);

        return new PersistedPreparedJournal(
            PersistenceId,
            LedgerId,
            PostingReference,
            JournalReference,
            JournalNumber,
            PostingDate,
            JournalLifecycleState.Cancelled.ToString(),
            CreatedAtUtc,
            ValidatedAtUtc,
            PostedAtUtc,
            ReversedAtUtc,
            cancelledAtUtc,
            reason,
            LedgerTransactionId,
            updatedJournal);
    }

    public static PersistedPreparedJournal Rehydrate(
        Guid persistenceId,
        LedgerId ledgerId,
        string postingReference,
        string journalReference,
        string journalNumber,
        DateOnly postingDate,
        string state,
        DateTime createdAtUtc,
        DateTime? validatedAtUtc,
        DateTime? postedAtUtc,
        DateTime? reversedAtUtc,
        DateTime? cancelledAtUtc,
        string? cancellationReason,
        Guid? ledgerTransactionId,
        PreparedJournal preparedJournal)
    {
        ArgumentNullException.ThrowIfNull(ledgerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(journalReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(journalNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentNullException.ThrowIfNull(preparedJournal);

        JournalLifecycleStateParser.Parse(state);

        return new PersistedPreparedJournal(
            persistenceId,
            ledgerId,
            postingReference.Trim(),
            journalReference.Trim(),
            journalNumber.Trim(),
            postingDate,
            state.Trim(),
            createdAtUtc,
            validatedAtUtc,
            postedAtUtc,
            reversedAtUtc,
            cancelledAtUtc,
            cancellationReason,
            ledgerTransactionId,
            preparedJournal);
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{parameterName} must be UTC.");
        }
    }

    private static class JournalLifecycleStateParser
    {
        public static JournalLifecycleState Parse(string state)
        {
            if (Enum.TryParse<JournalLifecycleState>(state, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException($"Unsupported journal lifecycle state '{state}'.");
        }
    }
}
