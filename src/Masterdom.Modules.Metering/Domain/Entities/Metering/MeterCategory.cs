using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterCategory : ValueObject
{
    public static readonly MeterCategory Electricity = new("Electricity");
    public static readonly MeterCategory Water = new("Water");
    public static readonly MeterCategory Gas = new("Gas");
    public static readonly MeterCategory Thermal = new("Thermal");

    private MeterCategory(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MeterCategory Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "ELECTRICITY" => Electricity,
            "WATER" => Water,
            "GAS" => Gas,
            "THERMAL" => Thermal,
            _ => new MeterCategory(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
