using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterType : ValueObject
{
    public static readonly MeterType Mechanical = new("Mechanical");
    public static readonly MeterType Digital = new("Digital");
    public static readonly MeterType Smart = new("Smart");

    private MeterType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MeterType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "MECHANICAL" => Mechanical,
            "DIGITAL" => Digital,
            "SMART" => Smart,
            _ => new MeterType(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
