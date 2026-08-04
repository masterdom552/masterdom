using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents bill generation cycle.
/// </summary>
public sealed class BillingCycle : ValueObject
{
    public static readonly BillingCycle Monthly = new("Monthly");
    public static readonly BillingCycle Quarterly = new("Quarterly");
    public static readonly BillingCycle OneTime = new("OneTime");

    private BillingCycle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BillingCycle Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "MONTHLY" => Monthly,
            "QUARTERLY" => Quarterly,
            "ONETIME" => OneTime,
            _ => new BillingCycle(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
