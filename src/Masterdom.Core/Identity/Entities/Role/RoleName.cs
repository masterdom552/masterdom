using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents the display name of a role.
/// </summary>
public sealed class RoleName : ValueObject
{
    private RoleName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a role name.
    /// </summary>
    public static RoleName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (value.Length > 100)
        {
            throw new ArgumentException(
                "Role name cannot exceed 100 characters.",
                nameof(value));
        }

        return new RoleName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(RoleName name)
    {
        return name.Value;
    }
}
