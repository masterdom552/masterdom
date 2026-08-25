using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Payment.Domain.Entities.Payment;

public sealed class PaymentSource : ValueObject
{
    public static readonly PaymentSource Tenant = new("Tenant");
    public static readonly PaymentSource Landlord = new("Landlord");
    public static readonly PaymentSource Agency = new("Agency");
    public static readonly PaymentSource SystemCorrection = new("SystemCorrection");

    private PaymentSource(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PaymentSource Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "TENANT" => Tenant,
            "LANDLORD" => Landlord,
            "AGENCY" => Agency,
            "SYSTEMCORRECTION" => SystemCorrection,
            _ => new PaymentSource(value.Trim())
        };
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
