using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.RefreshToken;

/// <summary>
/// Represents the unique identifier of a refresh token.
/// </summary>
public sealed record RefreshTokenId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new refresh token identifier.
    /// </summary>
    public static RefreshTokenId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a refresh token identifier from an existing Guid.
    /// </summary>
    public static RefreshTokenId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "RefreshTokenId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a refresh token identifier.
    /// </summary>
    public static RefreshTokenId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
