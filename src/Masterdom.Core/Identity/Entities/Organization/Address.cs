using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents an organization's address.
/// </summary>
public sealed class Address : ValueObject
{
    private Address(
        string type,
        string line1,
        string? line2,
        string? landmark,
        string city,
        string district,
        string state,
        string country,
        string postalCode,
        bool isPrimary,
        string? remarks,
        string? other)
    {
        Type = type;
        Line1 = line1;
        Line2 = line2;
        Landmark = landmark;
        City = city;
        District = district;
        State = state;
        Country = country;
        PostalCode = postalCode;
        IsPrimary = isPrimary;
        Remarks = remarks;
        Other = other;
    }

    /// <summary>
    /// Gets the address type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets address line 1.
    /// </summary>
    public string Line1 { get; }

    /// <summary>
    /// Gets address line 2.
    /// </summary>
    public string? Line2 { get; }

    /// <summary>
    /// Gets the landmark.
    /// </summary>
    public string? Landmark { get; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the district.
    /// </summary>
    public string District { get; }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public string State { get; }

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Gets whether this is the primary address.
    /// </summary>
    public bool IsPrimary { get; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; }

    /// <summary>
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; }

    /// <summary>
    /// Creates a new address.
    /// </summary>
    public static Address Create(
        string type,
        string line1,
        string city,
        string district,
        string state,
        string country,
        string postalCode,
        string? line2 = null,
        string? landmark = null,
        bool isPrimary = false,
        string? remarks = null,
        string? other = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        return new Address(
            type.Trim(),
            line1.Trim(),
            string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
            string.IsNullOrWhiteSpace(landmark) ? null : landmark.Trim(),
            city.Trim(),
            district.Trim(),
            state.Trim(),
            country.Trim(),
            postalCode.Trim(),
            isPrimary,
            string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
            string.IsNullOrWhiteSpace(other) ? null : other.Trim());
    }

    /// <summary>
    /// Marks this as the primary address.
    /// </summary>
    public Address MakePrimary()
    {
        if (IsPrimary)
            return this;

        return new Address(
            Type,
            Line1,
            Line2,
            Landmark,
            City,
            District,
            State,
            Country,
            PostalCode,
            true,
            Remarks,
            Other);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type.ToUpperInvariant();
        yield return Line1.ToUpperInvariant();
        yield return Line2?.ToUpperInvariant();
        yield return City.ToUpperInvariant();
        yield return District.ToUpperInvariant();
        yield return State.ToUpperInvariant();
        yield return Country.ToUpperInvariant();
        yield return PostalCode.ToUpperInvariant();
    }

    public override string ToString()
    {
        return $"{Line1}, {City}, {State}, {PostalCode}";
    }
}
