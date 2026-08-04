using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.LoginAttempt;

/// <summary>
/// Represents a user authentication attempt.
/// </summary>
public sealed class LoginAttempt : AggregateRoot<LoginAttemptId>
{
    private LoginAttempt(
        LoginAttemptId id,
        UserId? userId,
        string username,
        DateTime attemptedAtUtc,
        string? ipAddress,
        string? clientName)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        UserId = userId;

        Username = username.Trim();

        AttemptedAtUtc = attemptedAtUtc;

        IpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? null
            : ipAddress.Trim();

        ClientName = string.IsNullOrWhiteSpace(clientName)
            ? null
            : clientName.Trim();

        Status = LoginAttemptStatus.Pending;

        FailureReason = null;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new login attempt.
    /// </summary>
    public static LoginAttempt Create(
        string username,
        UserId? userId = null,
        string? ipAddress = null,
        string? clientName = null)
    {
        return new LoginAttempt(
            LoginAttemptId.New(),
            userId,
            username,
            DateTime.UtcNow,
            ipAddress,
            clientName);
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    public UserId? UserId { get; }

    /// <summary>
    /// Gets the username supplied.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets when the attempt occurred.
    /// </summary>
    public DateTime AttemptedAtUtc { get; }

    /// <summary>
    /// Gets the IP address.
    /// </summary>
    public string? IpAddress { get; }

    /// <summary>
    /// Gets the client application/browser.
    /// </summary>
    public string? ClientName { get; }

    /// <summary>
    /// Gets the login status.
    /// </summary>
    public LoginAttemptStatus Status { get; private set; }

    /// <summary>
    /// Gets the failure reason.
    /// </summary>
    public string? FailureReason { get; private set; }

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
    /// Marks the login attempt as successful.
    /// </summary>
    public void MarkSuccessful()
    {
        if (Status != LoginAttemptStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending login attempt can be completed.");
        }

        Status = LoginAttemptStatus.Successful;

        FailureReason = null;
    }

    /// <summary>
    /// Marks the login attempt as failed.
    /// </summary>
    public void MarkFailed(string? reason = null)
    {
        if (Status != LoginAttemptStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending login attempt can be completed.");
        }

        Status = LoginAttemptStatus.Failed;

        FailureReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
    }

    /// <summary>
    /// Marks the login attempt as blocked.
    /// </summary>
    public void MarkBlocked(string? reason = null)
    {
        if (Status != LoginAttemptStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending login attempt can be completed.");
        }

        Status = LoginAttemptStatus.Blocked;

        FailureReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
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
    /// Hides the login attempt.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the login attempt.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
