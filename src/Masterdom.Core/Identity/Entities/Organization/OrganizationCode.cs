using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents the business code of an organization.
/// </summary>
public sealed class OrganizationCode : ValueObject
{
    private OrganizationCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the organization code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an organization code.
    /// </summary>
    public static OrganizationCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "Organization code cannot exceed 50 characters.",
                nameof(value));
        }

        return new OrganizationCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(OrganizationCode code)
    {
        return code.Value;
    }
}
