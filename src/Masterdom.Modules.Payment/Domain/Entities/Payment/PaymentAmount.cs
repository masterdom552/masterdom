using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentAmount : ValueObject
{
    private PaymentAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static PaymentAmount Create(decimal value)
    {
        if (value < 0m)
        {
            throw new InvalidOperationException("Payment amount cannot be negative.");
        }

        return new PaymentAmount(decimal.Round(value, 2, MidpointRounding.AwayFromZero));
    }

    public static PaymentAmount Zero()
    {
        return new PaymentAmount(0m);
    }

    public PaymentAmount Add(PaymentAmount other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Create(Value + other.Value);
    }

    public PaymentAmount Subtract(PaymentAmount other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var remaining = Value - other.Value;
        if (remaining < 0m)
        {
            throw new InvalidOperationException("Payment amount cannot be negative.");
        }

        return Create(remaining);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
