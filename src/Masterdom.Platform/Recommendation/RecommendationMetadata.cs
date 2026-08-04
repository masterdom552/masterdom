using System.Collections.ObjectModel;

namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationMetadata
{
    public RecommendationMetadata(
        DateTime createdAtUtc,
        DateTime effectiveDateUtc,
        string version,
        string source,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recommendation metadata createdAtUtc must be UTC.");
        }

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Recommendation metadata effectiveDateUtc must be UTC.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Recommendation version cannot be empty.", nameof(version));
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Recommendation source cannot be empty.", nameof(source));
        }

        CreatedAtUtc = createdAtUtc;
        EffectiveDateUtc = effectiveDateUtc;
        Version = version.Trim();
        Source = source.Trim();
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public DateTime CreatedAtUtc { get; }

    public DateTime EffectiveDateUtc { get; }

    public string Version { get; }

    public string Source { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
