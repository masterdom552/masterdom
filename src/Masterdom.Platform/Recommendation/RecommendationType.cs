namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationType : IEquatable<RecommendationType>
{
    private RecommendationType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RecommendationType Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Recommendation type cannot be empty.", nameof(value));
        }

        return new RecommendationType(value.Trim());
    }

    public bool Equals(RecommendationType? other)
    {
        return other is not null &&
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is RecommendationType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }
}
