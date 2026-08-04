namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationPriority : IEquatable<RecommendationPriority>
{
    private RecommendationPriority(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static RecommendationPriority Create(int value)
    {
        if (value < 1 || value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Recommendation priority must be between 1 and 100.");
        }

        return new RecommendationPriority(value);
    }

    public bool Equals(RecommendationPriority? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecommendationPriority other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value;
    }
}
