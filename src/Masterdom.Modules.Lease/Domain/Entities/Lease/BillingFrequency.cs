using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents billing frequency for lease rent.
/// </summary>
public sealed class BillingFrequency : ValueObject
{
    public static readonly BillingFrequency Monthly = new("Monthly");
    public static readonly BillingFrequency Quarterly = new("Quarterly");
    public static readonly BillingFrequency Yearly = new("Yearly");

    private BillingFrequency(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BillingFrequency Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "MONTHLY" => Monthly,
            "QUARTERLY" => Quarterly,
            "YEARLY" => Yearly,
            _ => new BillingFrequency(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
