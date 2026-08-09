using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents a reusable party address.
/// </summary>
public sealed class Address : ValueObject
{
    private Address(
        AddressType type,
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string country,
        bool isPreferred)
    {
        Type = type;
        Line1 = NormalizeRequired(line1, nameof(line1));
        Line2 = NormalizeOptional(line2);
        City = NormalizeRequired(city, nameof(city));
        StateOrProvince = NormalizeRequired(stateOrProvince, nameof(stateOrProvince));
        PostalCode = NormalizeRequired(postalCode, nameof(postalCode));
        Country = NormalizeRequired(country, nameof(country));
        IsPreferred = isPreferred;
    }

    public AddressType Type { get; }

    public string Line1 { get; }

    public string? Line2 { get; }

    public string City { get; }

    public string StateOrProvince { get; }

    public string PostalCode { get; }

    public string Country { get; }

    public bool IsPreferred { get; }

    public static Address Create(
        string type,
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string country,
        bool isPreferred = false)
    {
        return Create(
            AddressType.Create(type),
            line1,
            line2,
            city,
            stateOrProvince,
            postalCode,
            country,
            isPreferred);
    }

    public static Address Create(
        AddressType type,
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string country,
        bool isPreferred = false)
    {
        ArgumentNullException.ThrowIfNull(type);

        return new Address(
            type,
            line1,
            line2,
            city,
            stateOrProvince,
            postalCode,
            country,
            isPreferred);
    }

    public Address WithPreferred(bool isPreferred)
    {
        return new Address(Type, Line1, Line2, City, StateOrProvince, PostalCode, Country, isPreferred);
    }

    public bool Matches(Address other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Type == other.Type
            && string.Equals(Line1, other.Line1, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Line2, other.Line2, StringComparison.OrdinalIgnoreCase)
            && string.Equals(City, other.City, StringComparison.OrdinalIgnoreCase)
            && string.Equals(StateOrProvince, other.StateOrProvince, StringComparison.OrdinalIgnoreCase)
            && string.Equals(PostalCode, other.PostalCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Country, other.Country, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Line1.ToUpperInvariant();
        yield return Line2?.ToUpperInvariant();
        yield return City.ToUpperInvariant();
        yield return StateOrProvince.ToUpperInvariant();
        yield return PostalCode.ToUpperInvariant();
        yield return Country.ToUpperInvariant();
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
