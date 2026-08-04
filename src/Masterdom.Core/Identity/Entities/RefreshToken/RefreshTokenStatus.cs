using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.RefreshToken;

/// <summary>
/// Represents the lifecycle status of a refresh token.
/// </summary>
public sealed class RefreshTokenStatus : ValueObject
{
    public static readonly RefreshTokenStatus Active = new("Active");
    public static readonly RefreshTokenStatus Used = new("Used");
    public static readonly RefreshTokenStatus Expired = new("Expired");
    public static readonly RefreshTokenStatus Revoked = new("Revoked");

    private RefreshTokenStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the refresh token status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a refresh token status.
    /// </summary>
    public static RefreshTokenStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "USED" => Used,
            "EXPIRED" => Expired,
            "REVOKED" => Revoked,
            _ => new RefreshTokenStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
