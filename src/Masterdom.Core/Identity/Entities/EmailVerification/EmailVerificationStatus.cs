using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.EmailVerification;

/// <summary>
/// Represents the lifecycle status of an email verification request.
/// </summary>
public sealed class EmailVerificationStatus : ValueObject
{
    public static readonly EmailVerificationStatus Pending = new("Pending");
    public static readonly EmailVerificationStatus Verified = new("Verified");
    public static readonly EmailVerificationStatus Cancelled = new("Cancelled");
    public static readonly EmailVerificationStatus Expired = new("Expired");

    private EmailVerificationStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the email verification status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an email verification status.
    /// </summary>
    public static EmailVerificationStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PENDING" => Pending,
            "VERIFIED" => Verified,
            "CANCELLED" => Cancelled,
            "EXPIRED" => Expired,
            _ => new EmailVerificationStatus(value)
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
