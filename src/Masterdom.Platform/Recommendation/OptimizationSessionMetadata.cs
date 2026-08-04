using System.Collections.ObjectModel;

namespace Masterdom.Platform.Recommendation;

public sealed class OptimizationSessionMetadata
{
    public OptimizationSessionMetadata(
        DateTime createdAtUtc,
        DateTime effectiveDateUtc,
        string contextVersion,
        string recommendationVersion,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Optimization session metadata createdAtUtc must be UTC.");
        }

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Optimization session metadata effectiveDateUtc must be UTC.");
        }

        if (string.IsNullOrWhiteSpace(contextVersion))
        {
            throw new ArgumentException("Context version cannot be empty.", nameof(contextVersion));
        }

        if (string.IsNullOrWhiteSpace(recommendationVersion))
        {
            throw new ArgumentException("Recommendation version cannot be empty.", nameof(recommendationVersion));
        }

        CreatedAtUtc = createdAtUtc;
        EffectiveDateUtc = effectiveDateUtc;
        ContextVersion = contextVersion.Trim();
        RecommendationVersion = recommendationVersion.Trim();
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public DateTime CreatedAtUtc { get; }

    public DateTime EffectiveDateUtc { get; }

    public string ContextVersion { get; }

    public string RecommendationVersion { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
