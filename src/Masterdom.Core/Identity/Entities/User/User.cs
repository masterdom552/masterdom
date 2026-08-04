using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.User;

/// <summary>
/// Represents an application user.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        UserCode code,
        IdentityProfileId identityProfileId,
        Username username)
        : base(id)
    {
        Code = code;
        IdentityProfileId = identityProfileId;
        Username = username;

        Status = UserStatus.Active;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new application user.
    /// </summary>
    public static User Create(
        UserCode code,
        IdentityProfileId identityProfileId,
        Username username)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(identityProfileId);
        ArgumentNullException.ThrowIfNull(username);

        return new User(
            UserId.New(),
            code,
            identityProfileId,
            username);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public UserCode Code { get; }

    /// <summary>
    /// Gets the linked identity profile.
    /// </summary>
    public IdentityProfileId IdentityProfileId { get; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public Username Username { get; private set; }

    /// <summary>
    /// Gets the lifecycle status.
    /// </summary>
    public UserStatus Status { get; private set; }

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
    /// Gets the effective start date.
    /// </summary>
    public DateTime? EffectiveFromUtc { get; private set; }

    /// <summary>
    /// Gets the effective end date.
    /// </summary>
    public DateTime? EffectiveToUtc { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the user is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Changes the username.
    /// </summary>
    public void ChangeUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);

        if (Username == username)
            return;

        Username = username;
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
    /// Changes remarks.
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
    /// Sets the effective period.
    /// </summary>
    public void SetEffectivePeriod(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc.HasValue &&
            toUtc.HasValue &&
            fromUtc > toUtc)
        {
            throw new InvalidOperationException(
                "EffectiveFromUtc cannot be after EffectiveToUtc.");
        }

        EffectiveFromUtc = fromUtc;
        EffectiveToUtc = toUtc;
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Hides the user.
    /// </summary>
    public void Hide() => IsHidden = true;

    /// <summary>
    /// Shows the user.
    /// </summary>
    public void Show() => IsHidden = false;

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate() => Status = UserStatus.Active;

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Archived)
            throw new InvalidOperationException(
                "An archived user cannot be deactivated.");

        Status = UserStatus.Inactive;
    }

    /// <summary>
    /// Archives the user.
    /// </summary>
    public void Archive() => Status = UserStatus.Archived;
}