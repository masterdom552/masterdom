namespace Masterdom.Platform.Recommendation;

/// <summary>
/// Identifies a recommendation instance.
/// </summary>
public sealed class RecommendationId : IEquatable<RecommendationId>
{
    private RecommendationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RecommendationId New()
    {
        return new RecommendationId(Guid.CreateVersion7());
    }

    public static RecommendationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RecommendationId cannot be empty.", nameof(value));
        }

        return new RecommendationId(value);
    }

    public bool Equals(RecommendationId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecommendationId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString("D");
    }
}
