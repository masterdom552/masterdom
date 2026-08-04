using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents the business code of a role.
/// </summary>
public sealed class RoleCode : ValueObject
{
    private RoleCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the role code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a role code.
    /// </summary>
    public static RoleCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "Role code cannot exceed 50 characters.",
                nameof(value));
        }

        return new RoleCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(RoleCode code)
    {
        return code.Value;
    }
}
