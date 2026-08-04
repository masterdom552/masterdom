using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.PasswordReset;

/// <summary>
/// Represents the lifecycle status of a password reset request.
/// </summary>
public sealed class PasswordResetStatus : ValueObject
{
    public static readonly PasswordResetStatus Pending = new("Pending");
    public static readonly PasswordResetStatus Completed = new("Completed");
    public static readonly PasswordResetStatus Cancelled = new("Cancelled");
    public static readonly PasswordResetStatus Expired = new("Expired");

    private PasswordResetStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the password reset status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a password reset status.
    /// </summary>
    public static PasswordResetStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PENDING" => Pending,
            "COMPLETED" => Completed,
            "CANCELLED" => Cancelled,
            "EXPIRED" => Expired,
            _ => new PasswordResetStatus(value)
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
