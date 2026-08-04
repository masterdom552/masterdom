using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterStatus : ValueObject
{
    public static readonly MeterStatus Installed = new("Installed");
    public static readonly MeterStatus Active = new("Active");
    public static readonly MeterStatus Retired = new("Retired");

    private MeterStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MeterStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "INSTALLED" => Installed,
            "ACTIVE" => Active,
            "RETIRED" => Retired,
            _ => new MeterStatus(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
