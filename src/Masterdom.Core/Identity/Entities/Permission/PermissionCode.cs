using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Permission;

/// <summary>
/// Represents the business code of a permission.
/// </summary>
public sealed class PermissionCode : ValueObject
{
    private PermissionCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the permission code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a permission code.
    /// </summary>
    public static PermissionCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 100)
        {
            throw new ArgumentException(
                "Permission code cannot exceed 100 characters.",
                nameof(value));
        }

        return new PermissionCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PermissionCode code)
    {
        return code.Value;
    }
}
