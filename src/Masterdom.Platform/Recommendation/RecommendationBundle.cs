namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationBundle
{
    private RecommendationBundle(
        RecommendationBundleId id,
        RecommendationBundleStatus status,
        IReadOnlyList<Recommendation> recommendations,
        DateTime createdAtUtc,
        DateTime effectiveDateUtc,
        string version,
        DecisionId? decisionId)
    {
        Id = id;
        Status = status;
        Recommendations = recommendations;
        CreatedAtUtc = createdAtUtc;
        EffectiveDateUtc = effectiveDateUtc;
        Version = version;
        DecisionId = decisionId;
    }

    public RecommendationBundleId Id { get; }

    public RecommendationBundleStatus Status { get; }

    public IReadOnlyList<Recommendation> Recommendations { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime EffectiveDateUtc { get; }

    public string Version { get; }

    public DecisionId? DecisionId { get; }

    public static RecommendationBundle CreateDraft(
        RecommendationBundleId id,
        IReadOnlyList<Recommendation> recommendations,
        DateTime createdAtUtc,
        DateTime effectiveDateUtc,
        string version)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(recommendations);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Bundle createdAtUtc must be UTC.");
        }

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Bundle effectiveDateUtc must be UTC.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Bundle version cannot be empty.", nameof(version));
        }

        return new RecommendationBundle(
            id,
            RecommendationBundleStatus.Draft,
            recommendations.ToArray(),
            createdAtUtc,
            effectiveDateUtc,
            version.Trim(),
            decisionId: null);
    }

    public RecommendationBundle Open()
    {
        if (Status != RecommendationBundleStatus.Draft)
        {
            throw new InvalidOperationException("Only draft bundles can be opened.");
        }

        return WithStatus(RecommendationBundleStatus.Open, null);
    }

    public RecommendationBundle FinalizeBundle()
    {
        if (Status != RecommendationBundleStatus.Open)
        {
            throw new InvalidOperationException("Only open bundles can be finalized.");
        }

        return WithStatus(RecommendationBundleStatus.Finalized, null);
    }

    public RecommendationBundle MarkDecided(DecisionId decisionId)
    {
        ArgumentNullException.ThrowIfNull(decisionId);

        if (Status != RecommendationBundleStatus.Finalized)
        {
            throw new InvalidOperationException("Only finalized bundles can be marked decided.");
        }

        return WithStatus(RecommendationBundleStatus.Decided, decisionId);
    }

    private RecommendationBundle WithStatus(RecommendationBundleStatus status, DecisionId? decisionId)
    {
        return new RecommendationBundle(
            Id,
            status,
            Recommendations,
            CreatedAtUtc,
            EffectiveDateUtc,
            Version,
            decisionId ?? DecisionId);
    }
}
