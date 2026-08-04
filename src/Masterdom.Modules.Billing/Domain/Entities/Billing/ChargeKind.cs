using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents supported billing charge categories.
/// </summary>
public sealed class ChargeKind : ValueObject
{
    public static readonly ChargeKind Rent = new("Rent");
    public static readonly ChargeKind UtilityReference = new("UtilityReference");
    public static readonly ChargeKind Maintenance = new("Maintenance");
    public static readonly ChargeKind Recurring = new("Recurring");
    public static readonly ChargeKind OneTime = new("OneTime");
    public static readonly ChargeKind CarryForward = new("CarryForward");

    private ChargeKind(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ChargeKind Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "RENT" => Rent,
            "UTILITYREFERENCE" => UtilityReference,
            "MAINTENANCE" => Maintenance,
            "RECURRING" => Recurring,
            "ONETIME" => OneTime,
            "CARRYFORWARD" => CarryForward,
            _ => new ChargeKind(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
