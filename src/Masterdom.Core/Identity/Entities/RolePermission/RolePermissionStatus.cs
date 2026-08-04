using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.RolePermission;

/// <summary>
/// Represents the lifecycle status of a role-permission assignment.
/// </summary>
public sealed class RolePermissionStatus : ValueObject
{
    public static readonly RolePermissionStatus Active = new("Active");
    public static readonly RolePermissionStatus Inactive = new("Inactive");
    public static readonly RolePermissionStatus Archived = new("Archived");

    private RolePermissionStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the role-permission assignment status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a role-permission assignment status.
    /// </summary>
    public static RolePermissionStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new RolePermissionStatus(value)
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
