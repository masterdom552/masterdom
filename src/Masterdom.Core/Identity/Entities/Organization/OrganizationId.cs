using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Organization;

/// <summary>
/// Represents the unique identifier of an organization.
/// </summary>
public sealed record OrganizationId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new organization identifier.
    /// </summary>
    public static OrganizationId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an organization identifier from an existing Guid.
    /// </summary>
    public static OrganizationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "OrganizationId cannot be empty.",
                nameof(value));

        return new(value);
    }

    /// <summary>
    /// Parses a string into an organization identifier.
    /// </summary>
    public static OrganizationId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
