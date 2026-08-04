using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterNumber : ValueObject
{
    private MeterNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MeterNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 50)
        {
            throw new ArgumentException("Meter number cannot exceed 50 characters.", nameof(value));
        }

        return new MeterNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
