using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed record PaymentId(Guid Value) : EntityId(Value)
{
    public static PaymentId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static PaymentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
