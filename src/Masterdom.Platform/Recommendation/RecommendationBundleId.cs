namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationBundleId : IEquatable<RecommendationBundleId>
{
    private RecommendationBundleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RecommendationBundleId New()
    {
        return new RecommendationBundleId(Guid.CreateVersion7());
    }

    public static RecommendationBundleId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RecommendationBundleId cannot be empty.", nameof(value));
        }

        return new RecommendationBundleId(value);
    }

    public bool Equals(RecommendationBundleId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecommendationBundleId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
