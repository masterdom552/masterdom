using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationRecommendation : ValueObject
{
    private OptimizationRecommendation(
        RecommendationId recommendationId,
        string title,
        string details,
        RecommendationPriority priority,
        DateTime generatedAtUtc,
        bool isArchived,
        DateTime? archivedAtUtc,
        string? archivedReason)
    {
        RecommendationId = recommendationId;
        Title = title;
        Details = details;
        Priority = priority;
        GeneratedAtUtc = generatedAtUtc;
        IsArchived = isArchived;
        ArchivedAtUtc = archivedAtUtc;
        ArchivedReason = archivedReason;
    }

    public RecommendationId RecommendationId { get; }

    public string Title { get; }

    public string Details { get; }

    public RecommendationPriority Priority { get; }

    public DateTime GeneratedAtUtc { get; }

    public bool IsArchived { get; }

    public DateTime? ArchivedAtUtc { get; }

    public string? ArchivedReason { get; }

    public static OptimizationRecommendation Generate(
        RecommendationId recommendationId,
        string title,
        string details,
        RecommendationPriority priority,
        DateTime generatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(recommendationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(details);
        ArgumentNullException.ThrowIfNull(priority);

        if (generatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recommendation timestamp must be UTC.");
        }

        return new OptimizationRecommendation(
            recommendationId,
            title.Trim(),
            details.Trim(),
            priority,
            generatedAtUtc,
            false,
            null,
            null);
    }

    public OptimizationRecommendation Archive(string reason, DateTime archivedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (archivedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recommendation archive timestamp must be UTC.");
        }

        if (IsArchived)
        {
            return this;
        }

        return new OptimizationRecommendation(
            RecommendationId,
            Title,
            Details,
            Priority,
            GeneratedAtUtc,
            true,
            archivedAtUtc,
            reason.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RecommendationId;
        yield return Title;
        yield return Details;
        yield return Priority;
        yield return GeneratedAtUtc;
        yield return IsArchived;
        yield return ArchivedAtUtc;
        yield return ArchivedReason;
    }
}
