using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the type of a postal address.
/// </summary>
public sealed class AddressType : ValueObject
{
    public static readonly AddressType Residential = new("Residential");
    public static readonly AddressType Business = new("Business");
    public static readonly AddressType Billing = new("Billing");
    public static readonly AddressType RegisteredOffice = new("Registered Office");

    private AddressType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AddressType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "RESIDENTIAL" => Residential,
            "BUSINESS" => Business,
            "BILLING" => Billing,
            "REGISTERED OFFICE" => RegisteredOffice,
            _ => new AddressType(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
