using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents the unique identifier of a role.
/// </summary>
public sealed record RoleId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new role identifier.
    /// </summary>
    public static RoleId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a role identifier from an existing Guid.
    /// </summary>
    public static RoleId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a role identifier.
    /// </summary>
    public static RoleId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
