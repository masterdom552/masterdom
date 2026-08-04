namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationConfidence : IEquatable<RecommendationConfidence>
{
    private RecommendationConfidence(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static RecommendationConfidence Create(decimal value)
    {
        if (value < 0m || value > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Recommendation confidence must be within [0,1].");
        }

        return new RecommendationConfidence(value);
    }

    public bool Equals(RecommendationConfidence? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecommendationConfidence other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
