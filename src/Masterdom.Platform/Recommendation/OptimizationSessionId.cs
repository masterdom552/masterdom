namespace Masterdom.Platform.Recommendation;

public sealed class OptimizationSessionId : IEquatable<OptimizationSessionId>
{
    private OptimizationSessionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static OptimizationSessionId New()
    {
        return new OptimizationSessionId(Guid.CreateVersion7());
    }

    public static OptimizationSessionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("OptimizationSessionId cannot be empty.", nameof(value));
        }

        return new OptimizationSessionId(value);
    }

    public bool Equals(OptimizationSessionId? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is OptimizationSessionId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
