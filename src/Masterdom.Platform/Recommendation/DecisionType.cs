namespace Masterdom.Platform.Recommendation;

public sealed class DecisionType : IEquatable<DecisionType>
{
    private DecisionType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DecisionType Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Decision type cannot be empty.", nameof(value));
        }

        return new DecisionType(value.Trim());
    }

    public bool Equals(DecisionType? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is DecisionType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }
}
