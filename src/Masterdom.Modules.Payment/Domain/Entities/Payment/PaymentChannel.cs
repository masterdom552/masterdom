using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentChannel : ValueObject
{
    public static readonly PaymentChannel Counter = new("Counter");
    public static readonly PaymentChannel Import = new("Import");
    public static readonly PaymentChannel Portal = new("Portal");
    public static readonly PaymentChannel Adjustment = new("Adjustment");

    private PaymentChannel(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PaymentChannel Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "COUNTER" => Counter,
            "IMPORT" => Import,
            "PORTAL" => Portal,
            "ADJUSTMENT" => Adjustment,
            _ => new PaymentChannel(value.Trim())
        };
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
