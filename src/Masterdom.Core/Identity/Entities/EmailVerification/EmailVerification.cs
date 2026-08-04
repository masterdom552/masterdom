using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.EmailVerification;

/// <summary>
/// Represents an email verification request.
/// </summary>
public sealed class EmailVerification : AggregateRoot<EmailVerificationId>
{
    private EmailVerification(
        EmailVerificationId id,
        UserId userId,
        string emailAddress,
        string tokenHash,
        DateTime requestedAtUtc,
        DateTime expiresAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);

        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (expiresAtUtc <= requestedAtUtc)
        {
            throw new ArgumentException(
                "Expiration must be later than request time.",
                nameof(expiresAtUtc));
        }

        UserId = userId;

        EmailAddress = emailAddress.Trim();

        TokenHash = tokenHash.Trim();

        RequestedAtUtc = requestedAtUtc;

        ExpiresAtUtc = expiresAtUtc;

        Status = EmailVerificationStatus.Pending;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new email verification request.
    /// </summary>
    public static EmailVerification Create(
        UserId userId,
        string emailAddress,
        string tokenHash,
        TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;

        return new EmailVerification(
            EmailVerificationId.New(),
            userId,
            emailAddress,
            tokenHash,
            now,
            now.Add(lifetime));
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the email address to verify.
    /// </summary>
    public string EmailAddress { get; }

    /// <summary>
    /// Gets the hashed verification token.
    /// Never store the raw token.
    /// </summary>
    public string TokenHash { get; }

    /// <summary>
    /// Gets when the request was created.
    /// </summary>
    public DateTime RequestedAtUtc { get; }

    /// <summary>
    /// Gets when the request expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets when the email was verified.
    /// </summary>
    public DateTime? VerifiedAtUtc { get; private set; }

    /// <summary>
    /// Gets when the request was cancelled.
    /// </summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>
    /// Gets the current verification status.
    /// </summary>
    public EmailVerificationStatus Status { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the record is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Returns true if the verification request is still valid.
    /// </summary>
    public bool IsValid(DateTime utcNow)
    {
        return Status == EmailVerificationStatus.Pending &&
               utcNow <= ExpiresAtUtc;
    }    /// <summary>
         /// Marks the email address as verified.
         /// </summary>
    public void Verify(DateTime verifiedAtUtc)
    {
        if (Status != EmailVerificationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending email verification can be completed.");
        }

        if (verifiedAtUtc > ExpiresAtUtc)
        {
            throw new InvalidOperationException(
                "The email verification request has expired.");
        }

        VerifiedAtUtc = verifiedAtUtc;

        Status = EmailVerificationStatus.Verified;
    }

    /// <summary>
    /// Cancels the verification request.
    /// </summary>
    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status != EmailVerificationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending email verification can be cancelled.");
        }

        CancelledAtUtc = cancelledAtUtc;

        Status = EmailVerificationStatus.Cancelled;
    }

    /// <summary>
    /// Marks the verification request as expired.
    /// </summary>
    public void Expire()
    {
        if (Status == EmailVerificationStatus.Expired)
        {
            return;
        }

        Status = EmailVerificationStatus.Expired;
    }

    /// <summary>
    /// Extends the expiration time.
    /// </summary>
    public void Extend(DateTime expiresAtUtc)
    {
        if (expiresAtUtc <= RequestedAtUtc)
        {
            throw new InvalidOperationException(
                "Expiration must be later than the request time.");
        }

        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Changes the description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Changes internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    /// <summary>
    /// Changes the configurable other field.
    /// </summary>
    public void ChangeOther(string? other)
    {
        Other = string.IsNullOrWhiteSpace(other)
            ? null
            : other.Trim();
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder));
        }

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Hides the verification request.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the verification request.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
