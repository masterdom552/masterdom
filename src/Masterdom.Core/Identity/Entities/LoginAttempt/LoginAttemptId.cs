using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.LoginAttempt;

/// <summary>
/// Represents the unique identifier of a login attempt.
/// </summary>
public sealed record LoginAttemptId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new login attempt identifier.
    /// </summary>
    public static LoginAttemptId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a login attempt identifier from an existing Guid.
    /// </summary>
    public static LoginAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "LoginAttemptId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a login attempt identifier.
    /// </summary>
    public static LoginAttemptId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
