namespace Masterdom.Platform.CalculationEngine.Metadata;

public sealed class CalculationOperationDescriptorId : IEquatable<CalculationOperationDescriptorId>
{
    private CalculationOperationDescriptorId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CalculationOperationDescriptorId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("DescriptorId cannot be empty.", nameof(value));
        }

        return new CalculationOperationDescriptorId(value.Trim());
    }

    public bool IsDefault => string.IsNullOrWhiteSpace(Value);

    public bool Equals(CalculationOperationDescriptorId? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is CalculationOperationDescriptorId other && Equals(other);
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
