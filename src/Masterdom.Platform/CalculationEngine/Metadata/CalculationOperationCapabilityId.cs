namespace Masterdom.Platform.CalculationEngine.Metadata;

public sealed class CalculationOperationCapabilityId : IEquatable<CalculationOperationCapabilityId>
{
    private CalculationOperationCapabilityId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CalculationOperationCapabilityId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("CapabilityId cannot be empty.", nameof(value));
        }

        return new CalculationOperationCapabilityId(value.Trim());
    }

    public bool IsDefault => string.IsNullOrWhiteSpace(Value);

    public bool Equals(CalculationOperationCapabilityId? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is CalculationOperationCapabilityId other && Equals(other);
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
