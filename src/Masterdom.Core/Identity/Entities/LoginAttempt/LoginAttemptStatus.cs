using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.LoginAttempt;

/// <summary>
/// Represents the lifecycle status of a login attempt.
/// </summary>
public sealed class LoginAttemptStatus : ValueObject
{
    public static readonly LoginAttemptStatus Pending = new("Pending");
    public static readonly LoginAttemptStatus Successful = new("Successful");
    public static readonly LoginAttemptStatus Failed = new("Failed");
    public static readonly LoginAttemptStatus Blocked = new("Blocked");

    private LoginAttemptStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the login attempt status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a login attempt status.
    /// </summary>
    public static LoginAttemptStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PENDING" => Pending,
            "SUCCESSFUL" => Successful,
            "FAILED" => Failed,
            "BLOCKED" => Blocked,
            _ => new LoginAttemptStatus(value)
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
