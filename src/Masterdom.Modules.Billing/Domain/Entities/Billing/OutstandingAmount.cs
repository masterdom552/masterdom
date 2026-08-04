using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents outstanding bill amount.
/// </summary>
public sealed class OutstandingAmount : ValueObject
{
    private OutstandingAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static OutstandingAmount Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidOperationException("Outstanding amount cannot be negative.");
        }

        return new OutstandingAmount(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
