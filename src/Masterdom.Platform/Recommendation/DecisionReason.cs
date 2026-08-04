namespace Masterdom.Platform.Recommendation;

public sealed class DecisionReason : IEquatable<DecisionReason>
{
    private DecisionReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DecisionReason Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Decision reason cannot be empty.", nameof(value));
        }

        return new DecisionReason(value.Trim());
    }

    public bool Equals(DecisionReason? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is DecisionReason other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }
}
