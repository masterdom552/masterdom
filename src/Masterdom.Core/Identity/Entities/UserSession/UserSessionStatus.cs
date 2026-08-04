using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserSession;

/// <summary>
/// Represents the lifecycle status of a user session.
/// </summary>
public sealed class UserSessionStatus : ValueObject
{
    public static readonly UserSessionStatus Active = new("Active");
    public static readonly UserSessionStatus Ended = new("Ended");
    public static readonly UserSessionStatus Expired = new("Expired");
    public static readonly UserSessionStatus Revoked = new("Revoked");

    private UserSessionStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the session status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a user session status.
    /// </summary>
    public static UserSessionStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "ENDED" => Ended,
            "EXPIRED" => Expired,
            "REVOKED" => Revoked,
            _ => new UserSessionStatus(value)
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
