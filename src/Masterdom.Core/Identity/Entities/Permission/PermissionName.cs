using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Permission;

/// <summary>
/// Represents the display name of a permission.
/// </summary>
public sealed class PermissionName : ValueObject
{
    private PermissionName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the permission name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a permission name.
    /// </summary>
    public static PermissionName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (value.Length > 150)
        {
            throw new ArgumentException(
                "Permission name cannot exceed 150 characters.",
                nameof(value));
        }

        return new PermissionName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PermissionName permissionName)
    {
        return permissionName.Value;
    }
}
