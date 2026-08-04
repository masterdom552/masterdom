using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents the lifecycle status of a role.
/// </summary>
public sealed class RoleStatus : ValueObject
{
    public static readonly RoleStatus Active = new("Active");
    public static readonly RoleStatus Inactive = new("Inactive");
    public static readonly RoleStatus Archived = new("Archived");

    private RoleStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the role status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a role status.
    /// </summary>
    public static RoleStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new RoleStatus(value)
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
