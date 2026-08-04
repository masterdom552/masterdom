namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class PostingBatch
{
    private PostingBatch(Guid batchId, string batchReference, string sourceModule, PostingStatus postingStatus, DateTime createdAtUtc, DateTime? completedAtUtc, IReadOnlyList<Guid> transactionIds)
    {
        BatchId = batchId;
        BatchReference = batchReference;
        SourceModule = sourceModule;
        PostingStatus = postingStatus;
        CreatedAtUtc = createdAtUtc;
        CompletedAtUtc = completedAtUtc;
        TransactionIds = transactionIds;
    }

    public Guid BatchId { get; private set; }

    public string BatchReference { get; private set; }

    public string SourceModule { get; private set; }

    public PostingStatus PostingStatus { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyList<Guid> TransactionIds { get; private set; }

    public static PostingBatch Create(string batchReference, string sourceModule, Guid transactionId, DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);

        if (transactionId == Guid.Empty)
        {
            throw new InvalidOperationException("Posting batch transaction identifier cannot be empty.");
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Posting batch timestamp must be UTC.");
        }

        return new PostingBatch(
            Guid.CreateVersion7(),
            batchReference.Trim(),
            sourceModule.Trim(),
            PostingStatus.Posted,
            createdAtUtc,
            null,
            new[] { transactionId });
    }

    public PostingBatch AppendTransaction(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
        {
            throw new InvalidOperationException("Posting batch transaction identifier cannot be empty.");
        }

        if (TransactionIds.Contains(transactionId))
        {
            return this;
        }

        return new PostingBatch(
            BatchId,
            BatchReference,
            SourceModule,
            PostingStatus,
            CreatedAtUtc,
            CompletedAtUtc,
            TransactionIds.Concat([transactionId]).ToList().AsReadOnly());
    }

    public PostingBatch Complete(DateTime completedAtUtc)
    {
        if (completedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Posting batch completion timestamp must be UTC.");
        }

        return new PostingBatch(BatchId, BatchReference, SourceModule, PostingStatus.Completed, CreatedAtUtc, completedAtUtc, TransactionIds);
    }
}
