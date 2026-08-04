using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents the name of an organization.
/// </summary>
public sealed class OrganizationName : ValueObject
{
    private OrganizationName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the organization name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an organization name.
    /// </summary>
    public static OrganizationName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (value.Length > 200)
        {
            throw new ArgumentException(
                "Organization name cannot exceed 200 characters.",
                nameof(value));
        }

        return new OrganizationName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(OrganizationName name)
    {
        return name.Value;
    }
}
