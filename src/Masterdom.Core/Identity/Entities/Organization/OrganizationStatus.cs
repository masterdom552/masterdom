using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents the lifecycle status of an organization.
/// </summary>
public sealed class OrganizationStatus : ValueObject
{
    public static readonly OrganizationStatus Active = new("Active");
    public static readonly OrganizationStatus Inactive = new("Inactive");
    public static readonly OrganizationStatus Archived = new("Archived");

    private OrganizationStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an organization status.
    /// </summary>
    public static OrganizationStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new OrganizationStatus(value)
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
