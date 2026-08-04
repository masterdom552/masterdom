using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents supported adjustment categories.
/// </summary>
public sealed class AdjustmentKind : ValueObject
{
    public static readonly AdjustmentKind ManualAdjustment = new("ManualAdjustment");
    public static readonly AdjustmentKind Debit = new("Debit");
    public static readonly AdjustmentKind Discount = new("Discount");
    public static readonly AdjustmentKind Waiver = new("Waiver");

    private AdjustmentKind(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AdjustmentKind Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "MANUALADJUSTMENT" => ManualAdjustment,
            "DEBIT" => Debit,
            "DISCOUNT" => Discount,
            "WAIVER" => Waiver,
            _ => new AdjustmentKind(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
