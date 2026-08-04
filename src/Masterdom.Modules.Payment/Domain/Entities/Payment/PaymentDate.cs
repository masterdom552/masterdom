using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentDate : ValueObject
{
    private PaymentDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static PaymentDate Create(DateOnly value)
    {
        return new PaymentDate(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
