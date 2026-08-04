using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ExternalLogin;

/// <summary>
/// Represents an external identity provider linked to a user.
/// </summary>
public sealed class ExternalLogin : AggregateRoot<ExternalLoginId>
{
    private ExternalLogin(
        ExternalLoginId id,
        UserId userId,
        ExternalLoginProvider provider,
        string providerUserId,
        DateTime linkedAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(provider);

        ArgumentException.ThrowIfNullOrWhiteSpace(providerUserId);

        UserId = userId;

        Provider = provider;

        ProviderUserId = providerUserId.Trim();

        LinkedAtUtc = linkedAtUtc;

        LastUsedAtUtc = null;

        Status = ExternalLoginStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new external login.
    /// </summary>
    public static ExternalLogin Create(
        UserId userId,
        ExternalLoginProvider provider,
        string providerUserId)
    {
        return new ExternalLogin(
            ExternalLoginId.New(),
            userId,
            provider,
            providerUserId,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the owning user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the external provider.
    /// </summary>
    public ExternalLoginProvider Provider { get; }

    /// <summary>
    /// Gets the provider's user identifier.
    /// </summary>
    public string ProviderUserId { get; }

    /// <summary>
    /// Gets when the provider was linked.
    /// </summary>
    public DateTime LinkedAtUtc { get; }

    /// <summary>
    /// Gets when the provider was last used.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public ExternalLoginStatus Status { get; private set; }

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
    public bool IsHidden { get; private set; }    /// <summary>
                                                  /// Records successful use of the external login.
                                                  /// </summary>
    public void RecordUsage(DateTime usedAtUtc)
    {
        if (Status != ExternalLoginStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active external login can be used.");
        }

        LastUsedAtUtc = usedAtUtc;
    }

    /// <summary>
    /// Activates the external login.
    /// </summary>
    public void Activate()
    {
        if (Status == ExternalLoginStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived external login cannot be activated.");
        }

        Status = ExternalLoginStatus.Active;
    }

    /// <summary>
    /// Deactivates the external login.
    /// </summary>
    public void Deactivate()
    {
        if (Status == ExternalLoginStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived external login cannot be deactivated.");
        }

        Status = ExternalLoginStatus.Inactive;
    }

    /// <summary>
    /// Archives the external login.
    /// </summary>
    public void Archive()
    {
        Status = ExternalLoginStatus.Archived;
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
    /// Hides the external login.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the external login.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
