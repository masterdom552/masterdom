using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the type of a contact method.
/// </summary>
public sealed class ContactMethodType : ValueObject
{
    public static readonly ContactMethodType Phone = new("Phone");
    public static readonly ContactMethodType Email = new("Email");
    public static readonly ContactMethodType Website = new("Website");

    private ContactMethodType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ContactMethodType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PHONE" => Phone,
            "EMAIL" => Email,
            "WEBSITE" => Website,
            _ => new ContactMethodType(value)
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
