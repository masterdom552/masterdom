namespace Masterdom.Platform.Recommendation;

public sealed class DecisionId : IEquatable<DecisionId>
{
    private DecisionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static DecisionId New()
    {
        return new DecisionId(Guid.CreateVersion7());
    }

    public static DecisionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("DecisionId cannot be empty.", nameof(value));
        }

        return new DecisionId(value);
    }

    public bool Equals(DecisionId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is DecisionId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
