using System.Collections.ObjectModel;
using Masterdom.Abstractions.Financial.Posting;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PreparedJournal
{
    public PreparedJournal(
        string journalReference,
        string postingReference,
        string journalNumber,
        DateOnly postingDate,
        string currencyCode,
        string description,
        string batchReference,
        string sourceModule,
        Guid billId,
        string billNumber,
        IReadOnlyCollection<PreparedJournalLine> lines,
        JournalLifecycleState lifecycleState = JournalLifecycleState.Prepared,
        DateTime? validatedAtUtc = null,
        DateTime? postedAtUtc = null,
        DateTime? reversedAtUtc = null,
        DateTime? cancelledAtUtc = null,
        string? cancellationReason = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journalReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(journalNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(batchReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(billNumber);

        if (billId == Guid.Empty)
        {
            throw new ArgumentException("Bill identifier cannot be empty.", nameof(billId));
        }

        ArgumentNullException.ThrowIfNull(lines);
        var materializedLines = lines.ToList();
        if (materializedLines.Count == 0)
        {
            throw new ArgumentException("At least one journal line is required.", nameof(lines));
        }

        var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();
        if (normalizedCurrencyCode.Length != 3)
        {
            throw new ArgumentException("Currency code must use ISO-4217 alpha-3 format.", nameof(currencyCode));
        }

        var debitTotal = materializedLines
            .Where(x => x.Direction == FinancialPostingDirection.Debit)
            .Sum(x => x.Amount);

        var creditTotal = materializedLines
            .Where(x => x.Direction == FinancialPostingDirection.Credit)
            .Sum(x => x.Amount);

        if (debitTotal != creditTotal)
        {
            throw new InvalidOperationException("Prepared journal must be balanced.");
        }

        if (materializedLines.Any(x => !string.Equals(x.CurrencyCode, normalizedCurrencyCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Prepared journal lines must use one journal currency.");
        }

        ValidateLifecycle(
            lifecycleState,
            validatedAtUtc,
            postedAtUtc,
            reversedAtUtc,
            cancelledAtUtc,
            cancellationReason);

        JournalReference = journalReference.Trim();
        PostingReference = postingReference.Trim();
        JournalNumber = journalNumber.Trim();
        PostingDate = postingDate;
        CurrencyCode = normalizedCurrencyCode;
        Description = description.Trim();
        BatchReference = batchReference.Trim();
        SourceModule = sourceModule.Trim().ToLowerInvariant();
        BillId = billId;
        BillNumber = billNumber.Trim();
        Lines = materializedLines.AsReadOnly();
        DebitTotal = debitTotal;
        CreditTotal = creditTotal;
        LifecycleState = lifecycleState;
        ValidatedAtUtc = validatedAtUtc;
        PostedAtUtc = postedAtUtc;
        ReversedAtUtc = reversedAtUtc;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = string.IsNullOrWhiteSpace(cancellationReason)
            ? null
            : cancellationReason.Trim();

        var metadataMaterialized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (metadata is not null)
        {
            foreach (var entry in metadata)
            {
                metadataMaterialized[entry.Key] = entry.Value;
            }
        }

        Metadata = new ReadOnlyDictionary<string, string>(metadataMaterialized);
    }

    public string JournalReference { get; }

    public string PostingReference { get; }

    public string JournalNumber { get; }

    public DateOnly PostingDate { get; }

    public string CurrencyCode { get; }

    public string Description { get; }

    public string BatchReference { get; }

    public string SourceModule { get; }

    public Guid BillId { get; }

    public string BillNumber { get; }

    public IReadOnlyCollection<PreparedJournalLine> Lines { get; }

    public decimal DebitTotal { get; }

    public decimal CreditTotal { get; }

    public JournalLifecycleState LifecycleState { get; }

    public DateTime? ValidatedAtUtc { get; }

    public DateTime? PostedAtUtc { get; }

    public DateTime? ReversedAtUtc { get; }

    public DateTime? CancelledAtUtc { get; }

    public string? CancellationReason { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public PreparedJournal MarkValidated(DateTime validatedAtUtc)
    {
        EnsureUtc(validatedAtUtc, nameof(validatedAtUtc));

        if (LifecycleState != JournalLifecycleState.Prepared)
        {
            throw new InvalidOperationException($"Cannot transition journal from {LifecycleState} to {JournalLifecycleState.Validated}.");
        }

        return Clone(
            JournalLifecycleState.Validated,
            validatedAtUtc,
            PostedAtUtc,
            ReversedAtUtc,
            CancelledAtUtc,
            CancellationReason);
    }

    public PreparedJournal MarkPosted(DateTime postedAtUtc)
    {
        EnsureUtc(postedAtUtc, nameof(postedAtUtc));

        if (LifecycleState != JournalLifecycleState.Validated)
        {
            throw new InvalidOperationException($"Cannot transition journal from {LifecycleState} to {JournalLifecycleState.Posted}.");
        }

        return Clone(
            JournalLifecycleState.Posted,
            ValidatedAtUtc,
            postedAtUtc,
            ReversedAtUtc,
            CancelledAtUtc,
            CancellationReason);
    }

    public PreparedJournal MarkReversed(DateTime reversedAtUtc)
    {
        EnsureUtc(reversedAtUtc, nameof(reversedAtUtc));

        if (LifecycleState != JournalLifecycleState.Posted)
        {
            throw new InvalidOperationException($"Cannot transition journal from {LifecycleState} to {JournalLifecycleState.Reversed}.");
        }

        return Clone(
            JournalLifecycleState.Reversed,
            ValidatedAtUtc,
            PostedAtUtc,
            reversedAtUtc,
            CancelledAtUtc,
            CancellationReason);
    }

    public PreparedJournal MarkCancelled(DateTime cancelledAtUtc, string reason)
    {
        EnsureUtc(cancelledAtUtc, nameof(cancelledAtUtc));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (LifecycleState != JournalLifecycleState.Prepared && LifecycleState != JournalLifecycleState.Validated)
        {
            throw new InvalidOperationException($"Cannot transition journal from {LifecycleState} to {JournalLifecycleState.Cancelled}.");
        }

        return Clone(
            JournalLifecycleState.Cancelled,
            ValidatedAtUtc,
            PostedAtUtc,
            ReversedAtUtc,
            cancelledAtUtc,
            reason.Trim());
    }

    private PreparedJournal Clone(
        JournalLifecycleState lifecycleState,
        DateTime? validatedAtUtc,
        DateTime? postedAtUtc,
        DateTime? reversedAtUtc,
        DateTime? cancelledAtUtc,
        string? cancellationReason)
    {
        return new PreparedJournal(
            JournalReference,
            PostingReference,
            JournalNumber,
            PostingDate,
            CurrencyCode,
            Description,
            BatchReference,
            SourceModule,
            BillId,
            BillNumber,
            Lines,
            lifecycleState,
            validatedAtUtc,
            postedAtUtc,
            reversedAtUtc,
            cancelledAtUtc,
            cancellationReason,
            Metadata);
    }

    private static void ValidateLifecycle(
        JournalLifecycleState lifecycleState,
        DateTime? validatedAtUtc,
        DateTime? postedAtUtc,
        DateTime? reversedAtUtc,
        DateTime? cancelledAtUtc,
        string? cancellationReason)
    {
        if (validatedAtUtc.HasValue)
        {
            EnsureUtc(validatedAtUtc.Value, nameof(validatedAtUtc));
        }

        if (postedAtUtc.HasValue)
        {
            EnsureUtc(postedAtUtc.Value, nameof(postedAtUtc));
        }

        if (reversedAtUtc.HasValue)
        {
            EnsureUtc(reversedAtUtc.Value, nameof(reversedAtUtc));
        }

        if (cancelledAtUtc.HasValue)
        {
            EnsureUtc(cancelledAtUtc.Value, nameof(cancelledAtUtc));
        }

        switch (lifecycleState)
        {
            case JournalLifecycleState.Prepared:
                if (validatedAtUtc.HasValue || postedAtUtc.HasValue || reversedAtUtc.HasValue || cancelledAtUtc.HasValue)
                {
                    throw new InvalidOperationException("Prepared journals cannot have lifecycle timestamps.");
                }
                break;
            case JournalLifecycleState.Validated:
                if (!validatedAtUtc.HasValue || postedAtUtc.HasValue || reversedAtUtc.HasValue || cancelledAtUtc.HasValue)
                {
                    throw new InvalidOperationException("Validated journals must only have validation timestamp.");
                }
                break;
            case JournalLifecycleState.Posted:
                if (!validatedAtUtc.HasValue || !postedAtUtc.HasValue || reversedAtUtc.HasValue || cancelledAtUtc.HasValue)
                {
                    throw new InvalidOperationException("Posted journals must have validation and posting timestamps.");
                }
                break;
            case JournalLifecycleState.Reversed:
                if (!validatedAtUtc.HasValue || !postedAtUtc.HasValue || !reversedAtUtc.HasValue || cancelledAtUtc.HasValue)
                {
                    throw new InvalidOperationException("Reversed journals must have validation, posting, and reversal timestamps.");
                }
                break;
            case JournalLifecycleState.Cancelled:
                if (!cancelledAtUtc.HasValue)
                {
                    throw new InvalidOperationException("Cancelled journals must have cancellation timestamp.");
                }

                if (string.IsNullOrWhiteSpace(cancellationReason))
                {
                    throw new InvalidOperationException("Cancelled journals must include a cancellation reason.");
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported lifecycle state '{lifecycleState}'.");
        }
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{parameterName} must be UTC.");
        }
    }
}
