namespace Masterdom.Platform.Recommendation;

public sealed class Recommendation
{
    private Recommendation(
        RecommendationId id,
        RecommendationType type,
        RecommendationStatus status,
        RecommendationPriority priority,
        RecommendationConfidence confidence,
        RecommendationEvidence evidence,
        RecommendationExplanation explanation,
        RecommendationMetadata metadata,
        string? statusReason,
        DateTime? statusChangedAtUtc)
    {
        Id = id;
        Type = type;
        Status = status;
        Priority = priority;
        Confidence = confidence;
        Evidence = evidence;
        Explanation = explanation;
        Metadata = metadata;
        StatusReason = statusReason;
        StatusChangedAtUtc = statusChangedAtUtc;
    }

    public RecommendationId Id { get; }

    public RecommendationType Type { get; }

    public RecommendationStatus Status { get; }

    public RecommendationPriority Priority { get; }

    public RecommendationConfidence Confidence { get; }

    public RecommendationEvidence Evidence { get; }

    public RecommendationExplanation Explanation { get; }

    public RecommendationMetadata Metadata { get; }

    public string? StatusReason { get; }

    public DateTime? StatusChangedAtUtc { get; }

    public static Recommendation CreateDraft(
        RecommendationId id,
        RecommendationType type,
        RecommendationPriority priority,
        RecommendationConfidence confidence,
        RecommendationEvidence evidence,
        RecommendationExplanation explanation,
        RecommendationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(priority);
        ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(explanation);
        ArgumentNullException.ThrowIfNull(metadata);

        return new Recommendation(
            id,
            type,
            RecommendationStatus.Draft,
            priority,
            confidence,
            evidence,
            explanation,
            metadata,
            statusReason: null,
            statusChangedAtUtc: null);
    }

    public Recommendation MarkProposed(DateTime changedAtUtc)
    {
        EnsureUtc(changedAtUtc, nameof(changedAtUtc));

        if (Status != RecommendationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft recommendations can be proposed.");
        }

        return WithStatus(RecommendationStatus.Proposed, null, changedAtUtc);
    }

    public Recommendation Accept(string reason, DateTime changedAtUtc)
    {
        EnsureUtc(changedAtUtc, nameof(changedAtUtc));

        if (Status != RecommendationStatus.Proposed)
        {
            throw new InvalidOperationException("Only proposed recommendations can be accepted.");
        }

        return WithStatus(RecommendationStatus.Accepted, reason, changedAtUtc);
    }

    public Recommendation Reject(string reason, DateTime changedAtUtc)
    {
        EnsureUtc(changedAtUtc, nameof(changedAtUtc));

        if (Status != RecommendationStatus.Proposed)
        {
            throw new InvalidOperationException("Only proposed recommendations can be rejected.");
        }

        return WithStatus(RecommendationStatus.Rejected, reason, changedAtUtc);
    }

    public Recommendation Archive(string reason, DateTime changedAtUtc)
    {
        EnsureUtc(changedAtUtc, nameof(changedAtUtc));

        if (Status is RecommendationStatus.Archived)
        {
            throw new InvalidOperationException("Recommendation is already archived.");
        }

        return WithStatus(RecommendationStatus.Archived, reason, changedAtUtc);
    }

    private Recommendation WithStatus(RecommendationStatus status, string? reason, DateTime changedAtUtc)
    {
        return new Recommendation(
            Id,
            Type,
            status,
            Priority,
            Confidence,
            Evidence,
            Explanation,
            Metadata,
            statusReason: reason,
            statusChangedAtUtc: changedAtUtc);
    }

    private static void EnsureUtc(DateTime value, string argument)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException($"{argument} must be UTC.");
        }
    }
}
