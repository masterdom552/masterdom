using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.RolePermission;

/// <summary>
/// Represents the unique identifier of a role-permission assignment.
/// </summary>
public sealed record RolePermissionId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new role-permission identifier.
    /// </summary>
    public static RolePermissionId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a role-permission identifier from an existing Guid.
    /// </summary>
    public static RolePermissionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "RolePermissionId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a role-permission identifier.
    /// </summary>
    public static RolePermissionId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
