using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentReference : ValueObject
{
    private PaymentReference(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PaymentReference Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PaymentReference(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
