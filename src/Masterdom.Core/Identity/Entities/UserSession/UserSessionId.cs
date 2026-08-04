using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserSession;

/// <summary>
/// Represents the unique identifier of a user session.
/// </summary>
public sealed record UserSessionId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new user session identifier.
    /// </summary>
    public static UserSessionId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a user session identifier from an existing Guid.
    /// </summary>
    public static UserSessionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "UserSessionId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a user session identifier.
    /// </summary>
    public static UserSessionId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
