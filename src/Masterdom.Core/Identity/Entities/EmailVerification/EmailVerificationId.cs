using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.EmailVerification;

/// <summary>
/// Represents the unique identifier of an email verification request.
/// </summary>
public sealed record EmailVerificationId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new email verification identifier.
    /// </summary>
    public static EmailVerificationId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an email verification identifier from an existing Guid.
    /// </summary>
    public static EmailVerificationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "EmailVerificationId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into an email verification identifier.
    /// </summary>
    public static EmailVerificationId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
