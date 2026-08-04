using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.User;

/// <summary>
/// Represents the unique identifier of a user.
/// </summary>
public sealed record UserId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new user identifier.
    /// </summary>
    public static UserId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a user identifier from an existing Guid.
    /// </summary>
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a user identifier.
    /// </summary>
    public static UserId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
