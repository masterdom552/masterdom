namespace Masterdom.Platform.Recommendation;

public sealed class OptimizationSession
{
    private OptimizationSession(
        OptimizationSessionId id,
        OptimizationSessionStatus status,
        OptimizationSessionMetadata metadata,
        DateTime? startedAtUtc,
        DateTime? completedAtUtc,
        DateTime? cancelledAtUtc,
        string? cancellationReason)
    {
        Id = id;
        Status = status;
        Metadata = metadata;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = cancellationReason;
    }

    public OptimizationSessionId Id { get; }

    public OptimizationSessionStatus Status { get; }

    public OptimizationSessionMetadata Metadata { get; }

    public DateTime? StartedAtUtc { get; }

    public DateTime? CompletedAtUtc { get; }

    public DateTime? CancelledAtUtc { get; }

    public string? CancellationReason { get; }

    public static OptimizationSession Create(OptimizationSessionId id, OptimizationSessionMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(metadata);

        return new OptimizationSession(
            id,
            OptimizationSessionStatus.Created,
            metadata,
            startedAtUtc: null,
            completedAtUtc: null,
            cancelledAtUtc: null,
            cancellationReason: null);
    }

    public OptimizationSession Start(DateTime startedAtUtc)
    {
        EnsureUtc(startedAtUtc, nameof(startedAtUtc));

        if (Status != OptimizationSessionStatus.Created)
        {
            throw new InvalidOperationException("Only created sessions can start.");
        }

        return new OptimizationSession(
            Id,
            OptimizationSessionStatus.Running,
            Metadata,
            startedAtUtc,
            CompletedAtUtc,
            CancelledAtUtc,
            CancellationReason);
    }

    public OptimizationSession Complete(DateTime completedAtUtc)
    {
        EnsureUtc(completedAtUtc, nameof(completedAtUtc));

        if (Status != OptimizationSessionStatus.Running)
        {
            throw new InvalidOperationException("Only running sessions can complete.");
        }

        return new OptimizationSession(
            Id,
            OptimizationSessionStatus.Completed,
            Metadata,
            StartedAtUtc,
            completedAtUtc,
            CancelledAtUtc,
            CancellationReason);
    }

    public OptimizationSession Cancel(string reason, DateTime cancelledAtUtc)
    {
        EnsureUtc(cancelledAtUtc, nameof(cancelledAtUtc));

        if (Status is OptimizationSessionStatus.Completed or OptimizationSessionStatus.Cancelled)
        {
            throw new InvalidOperationException("Completed or cancelled sessions cannot be cancelled again.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Cancellation reason cannot be empty.", nameof(reason));
        }

        return new OptimizationSession(
            Id,
            OptimizationSessionStatus.Cancelled,
            Metadata,
            StartedAtUtc,
            CompletedAtUtc,
            cancelledAtUtc,
            reason.Trim());
    }

    private static void EnsureUtc(DateTime value, string argument)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{argument} must be UTC.");
        }
    }
}
