namespace Masterdom.Platform.CalculationEngine.Metadata;

public sealed class CalculationOperationVersion : IEquatable<CalculationOperationVersion>
{
    private CalculationOperationVersion(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CalculationOperationVersion Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("OperationVersion cannot be empty.", nameof(value));
        }

        return new CalculationOperationVersion(value.Trim());
    }

    public bool IsDefault => string.IsNullOrWhiteSpace(Value);

    public bool Equals(CalculationOperationVersion? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is CalculationOperationVersion other && Equals(other);
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
