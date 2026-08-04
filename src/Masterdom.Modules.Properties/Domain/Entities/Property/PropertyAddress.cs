using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents the normalized postal address of a property.
/// </summary>
public sealed class PropertyAddress : ValueObject
{
    public PropertyAddress(
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(stateOrProvince);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = city.Trim();
        StateOrProvince = stateOrProvince.Trim();
        PostalCode = postalCode.Trim().ToUpperInvariant();
        CountryCode = countryCode.Trim().ToUpperInvariant();

        if (CountryCode.Length is < 2 or > 3)
        {
            throw new ArgumentException("Country code must use ISO alpha-2 or alpha-3 format.", nameof(countryCode));
        }
    }

    public string Line1 { get; }

    public string? Line2 { get; }

    public string City { get; }

    public string StateOrProvince { get; }

    public string PostalCode { get; }

    public string CountryCode { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return StateOrProvince;
        yield return PostalCode;
        yield return CountryCode;
    }
}
