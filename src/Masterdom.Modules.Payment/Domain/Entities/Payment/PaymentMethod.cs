using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentMethod : ValueObject
{
    public static readonly PaymentMethod Cash = new("Cash");
    public static readonly PaymentMethod BankTransfer = new("BankTransfer");
    public static readonly PaymentMethod Check = new("Check");
    public static readonly PaymentMethod Card = new("Card");
    public static readonly PaymentMethod Manual = new("Manual");

    private PaymentMethod(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PaymentMethod Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "CASH" => Cash,
            "BANKTRANSFER" => BankTransfer,
            "CHECK" => Check,
            "CARD" => Card,
            "MANUAL" => Manual,
            _ => new PaymentMethod(value.Trim())
        };
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
