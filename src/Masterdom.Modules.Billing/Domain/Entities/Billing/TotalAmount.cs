using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents total bill amount.
/// </summary>
public sealed class TotalAmount : ValueObject
{
    private TotalAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static TotalAmount Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Total amount cannot be negative.");
        }

        return new TotalAmount(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
