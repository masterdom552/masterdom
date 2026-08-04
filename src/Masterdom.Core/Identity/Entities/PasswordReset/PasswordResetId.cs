using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.PasswordReset;

/// <summary>
/// Represents the unique identifier of a password reset request.
/// </summary>
public sealed record PasswordResetId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new password reset identifier.
    /// </summary>
    public static PasswordResetId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a password reset identifier from an existing Guid.
    /// </summary>
    public static PasswordResetId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "PasswordResetId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a password reset identifier.
    /// </summary>
    public static PasswordResetId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
