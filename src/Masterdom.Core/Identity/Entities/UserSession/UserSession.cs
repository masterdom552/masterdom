using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserSession;

/// <summary>
/// Represents an authenticated user session.
/// </summary>
public sealed class UserSession : AggregateRoot<UserSessionId>
{
    private UserSession(
        UserSessionId id,
        UserId userId,
        DateTime startedAtUtc,
        string? ipAddress,
        string? deviceName,
        string? clientName)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);

        UserId = userId;

        StartedAtUtc = startedAtUtc;

        LastActivityAtUtc = startedAtUtc;

        ExpiresAtUtc = startedAtUtc.AddDays(30);

        IpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? null
            : ipAddress.Trim();

        DeviceName = string.IsNullOrWhiteSpace(deviceName)
            ? null
            : deviceName.Trim();

        ClientName = string.IsNullOrWhiteSpace(clientName)
            ? null
            : clientName.Trim();

        Status = UserSessionStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new authenticated session.
    /// </summary>
    public static UserSession Create(
        UserId userId,
        string? ipAddress = null,
        string? deviceName = null,
        string? clientName = null)
    {
        return new UserSession(
            UserSessionId.New(),
            userId,
            DateTime.UtcNow,
            ipAddress,
            deviceName,
            clientName);
    }

    /// <summary>
    /// Gets the authenticated user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets when the session started.
    /// </summary>
    public DateTime StartedAtUtc { get; }

    /// <summary>
    /// Gets the last activity timestamp.
    /// </summary>
    public DateTime LastActivityAtUtc { get; private set; }

    /// <summary>
    /// Gets the session expiration timestamp.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets when the session ended.
    /// </summary>
    public DateTime? EndedAtUtc { get; private set; }

    /// <summary>
    /// Gets the client IP address.
    /// </summary>
    public string? IpAddress { get; }

    /// <summary>
    /// Gets the client device.
    /// </summary>
    public string? DeviceName { get; }

    /// <summary>
    /// Gets the client application/browser.
    /// </summary>
    public string? ClientName { get; }

    /// <summary>
    /// Gets the session status.
    /// </summary>
    public UserSessionStatus Status { get; private set; }

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
    /// Gets whether the session is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Returns true when the session is currently valid.
    /// </summary>
    public bool IsActive(DateTime utcNow)
    {
        return Status == UserSessionStatus.Active &&
               EndedAtUtc is null &&
               utcNow <= ExpiresAtUtc;
    }    /// <summary>
         /// Records activity for the session.
         /// </summary>
    public void RecordActivity(DateTime activityAtUtc)
    {
        if (Status != UserSessionStatus.Active)
        {
            throw new InvalidOperationException(
                "Cannot record activity for an inactive session.");
        }

        if (activityAtUtc < StartedAtUtc)
        {
            throw new InvalidOperationException(
                "Activity time cannot be earlier than session start.");
        }

        LastActivityAtUtc = activityAtUtc;
    }

    /// <summary>
    /// Extends the session expiration.
    /// </summary>
    public void Extend(DateTime expiresAtUtc)
    {
        if (expiresAtUtc <= LastActivityAtUtc)
        {
            throw new InvalidOperationException(
                "Expiration must be later than the last recorded activity.");
        }

        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Ends the session.
    /// </summary>
    public void End(DateTime endedAtUtc)
    {
        if (EndedAtUtc.HasValue)
        {
            throw new InvalidOperationException(
                "The session has already ended.");
        }

        if (endedAtUtc < StartedAtUtc)
        {
            throw new InvalidOperationException(
                "End time cannot be earlier than session start.");
        }

        EndedAtUtc = endedAtUtc;

        Status = UserSessionStatus.Ended;
    }

    /// <summary>
    /// Revokes the session immediately.
    /// </summary>
    public void Revoke()
    {
        if (Status == UserSessionStatus.Revoked)
            return;

        EndedAtUtc ??= DateTime.UtcNow;

        Status = UserSessionStatus.Revoked;
    }

    /// <summary>
    /// Expires the session.
    /// </summary>
    public void Expire()
    {
        if (Status == UserSessionStatus.Expired)
            return;

        EndedAtUtc ??= DateTime.UtcNow;

        Status = UserSessionStatus.Expired;
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
    /// Hides the session.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the session.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
