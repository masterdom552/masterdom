using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents a contact method owned by a party.
/// </summary>
public sealed class ContactMethod : ValueObject
{
    private ContactMethod(ContactMethodType type, string value, bool isPreferred)
    {
        Type = type;
        Value = NormalizeValue(value);
        IsPreferred = isPreferred;
    }

    public ContactMethodType Type { get; }

    public string Value { get; }

    public bool IsPreferred { get; }

    public static ContactMethod Create(string type, string value, bool isPreferred = false)
    {
        return Create(ContactMethodType.Create(type), value, isPreferred);
    }

    public static ContactMethod Create(ContactMethodType type, string value, bool isPreferred = false)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = NormalizeValue(value);

        if (type == ContactMethodType.Email && !normalizedValue.Contains('@'))
        {
            throw new ArgumentException("Email contact methods must contain '@'.", nameof(value));
        }

        if (type == ContactMethodType.Website &&
            !Uri.TryCreate(normalizedValue, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Website contact methods must be absolute URIs.", nameof(value));
        }

        return new ContactMethod(type, normalizedValue, isPreferred);
    }

    public ContactMethod WithPreferred(bool isPreferred)
    {
        return new ContactMethod(Type, Value, isPreferred);
    }

    public bool Matches(ContactMethod other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Type == other.Type
            && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Value.ToUpperInvariant();
    }

    private static string NormalizeValue(string value)
    {
        return value.Trim();
    }
}
