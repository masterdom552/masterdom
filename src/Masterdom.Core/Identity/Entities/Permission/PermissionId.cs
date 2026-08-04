using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Permission;

/// <summary>
/// Represents the unique identifier of a permission.
/// </summary>
public sealed record PermissionId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new permission identifier.
    /// </summary>
    public static PermissionId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a permission identifier from an existing Guid.
    /// </summary>
    public static PermissionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "PermissionId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a permission identifier.
    /// </summary>
    public static PermissionId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
