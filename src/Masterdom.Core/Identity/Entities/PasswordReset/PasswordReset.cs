using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.PasswordReset;

/// <summary>
/// Represents a password reset request.
/// </summary>
public sealed class PasswordReset : AggregateRoot<PasswordResetId>
{
    private PasswordReset(
        PasswordResetId id,
        UserId userId,
        string tokenHash,
        DateTime requestedAtUtc,
        DateTime expiresAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (expiresAtUtc <= requestedAtUtc)
        {
            throw new ArgumentException(
                "Expiration must be later than request time.",
                nameof(expiresAtUtc));
        }

        UserId = userId;

        TokenHash = tokenHash.Trim();

        RequestedAtUtc = requestedAtUtc;

        ExpiresAtUtc = expiresAtUtc;

        Status = PasswordResetStatus.Pending;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new password reset request.
    /// </summary>
    public static PasswordReset Create(
        UserId userId,
        string tokenHash,
        TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;

        return new PasswordReset(
            PasswordResetId.New(),
            userId,
            tokenHash,
            now,
            now.Add(lifetime));
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the hashed reset token.
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
    /// Gets when the password was successfully reset.
    /// </summary>
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Gets when the request was cancelled.
    /// </summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public PasswordResetStatus Status { get; private set; }

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
    /// Gets whether the request is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Returns true when the reset request is still valid.
    /// </summary>
    public bool IsValid(DateTime utcNow)
    {
        return Status == PasswordResetStatus.Pending &&
               utcNow <= ExpiresAtUtc;
    }    /// <summary>
         /// Marks the password reset as completed.
         /// </summary>
    public void Complete(DateTime completedAtUtc)
    {
        if (Status != PasswordResetStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending password reset can be completed.");
        }

        if (completedAtUtc > ExpiresAtUtc)
        {
            throw new InvalidOperationException(
                "The password reset request has expired.");
        }

        CompletedAtUtc = completedAtUtc;

        Status = PasswordResetStatus.Completed;
    }

    /// <summary>
    /// Cancels the password reset request.
    /// </summary>
    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status != PasswordResetStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending password reset can be cancelled.");
        }

        CancelledAtUtc = cancelledAtUtc;

        Status = PasswordResetStatus.Cancelled;
    }

    /// <summary>
    /// Marks the password reset request as expired.
    /// </summary>
    public void Expire()
    {
        if (Status == PasswordResetStatus.Expired)
        {
            return;
        }

        Status = PasswordResetStatus.Expired;
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
    /// Hides the password reset request.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the password reset request.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
