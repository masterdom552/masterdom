using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.MfaDevice;

/// <summary>
/// Represents a multi-factor authentication device registered by a user.
/// </summary>
public sealed class MfaDevice : AggregateRoot<MfaDeviceId>
{
    private MfaDevice(
        MfaDeviceId id,
        UserId userId,
        string name,
        MfaDeviceType type,
        string secretHash,
        DateTime registeredAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(type);

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretHash);

        UserId = userId;

        Name = name.Trim();

        Type = type;

        SecretHash = secretHash.Trim();

        RegisteredAtUtc = registeredAtUtc;

        Status = MfaDeviceStatus.Pending;

        VerifiedAtUtc = null;

        LastUsedAtUtc = null;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new MFA device.
    /// </summary>
    public static MfaDevice Create(
        UserId userId,
        string name,
        MfaDeviceType type,
        string secretHash)
    {
        return new MfaDevice(
            MfaDeviceId.New(),
            userId,
            name,
            type,
            secretHash,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the owning user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the MFA device type.
    /// </summary>
    public MfaDeviceType Type { get; }

    /// <summary>
    /// Gets the hashed secret.
    /// Never store the raw secret.
    /// </summary>
    public string SecretHash { get; }

    /// <summary>
    /// Gets when the device was registered.
    /// </summary>
    public DateTime RegisteredAtUtc { get; }

    /// <summary>
    /// Gets when the device was verified.
    /// </summary>
    public DateTime? VerifiedAtUtc { get; private set; }

    /// <summary>
    /// Gets when the device was last used.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; private set; }

    /// <summary>
    /// Gets the device status.
    /// </summary>
    public MfaDeviceStatus Status { get; private set; }

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
                                                  /// Marks the MFA device as verified.
                                                  /// </summary>
    public void Verify(DateTime verifiedAtUtc)
    {
        if (Status != MfaDeviceStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only a pending MFA device can be verified.");
        }

        VerifiedAtUtc = verifiedAtUtc;

        Status = MfaDeviceStatus.Active;
    }

    /// <summary>
    /// Records successful use of the MFA device.
    /// </summary>
    public void RecordUsage(DateTime usedAtUtc)
    {
        if (Status != MfaDeviceStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active MFA device can be used.");
        }

        LastUsedAtUtc = usedAtUtc;
    }

    /// <summary>
    /// Renames the MFA device.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
    }

    /// <summary>
    /// Deactivates the MFA device.
    /// </summary>
    public void Deactivate()
    {
        if (Status == MfaDeviceStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived MFA device cannot be deactivated.");
        }

        Status = MfaDeviceStatus.Inactive;
    }

    /// <summary>
    /// Activates the MFA device.
    /// </summary>
    public void Activate()
    {
        if (Status == MfaDeviceStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived MFA device cannot be activated.");
        }

        Status = MfaDeviceStatus.Active;
    }

    /// <summary>
    /// Archives the MFA device.
    /// </summary>
    public void Archive()
    {
        Status = MfaDeviceStatus.Archived;
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
    /// Hides the MFA device.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the MFA device.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
