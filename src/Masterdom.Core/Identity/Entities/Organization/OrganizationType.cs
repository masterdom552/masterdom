using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents the type of an organization.
/// </summary>
public sealed class OrganizationType : ValueObject
{
    public static readonly OrganizationType Enterprise = new("Enterprise");
    public static readonly OrganizationType SmallBusiness = new("SmallBusiness");
    public static readonly OrganizationType NonProfit = new("NonProfit");

    private OrganizationType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the type value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an organization type.
    /// </summary>
    public static OrganizationType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ENTERPRISE" => Enterprise,
            "SMALLBUSINESS" => SmallBusiness,
            "NONPROFIT" => NonProfit,
            _ => new OrganizationType(value)
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
