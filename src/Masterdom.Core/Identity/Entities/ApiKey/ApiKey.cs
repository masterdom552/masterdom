using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ApiKey;

/// <summary>
/// Represents an API key issued to a user.
/// </summary>
public sealed class ApiKey : AggregateRoot<ApiKeyId>
{
    private ApiKey(
        ApiKeyId id,
        UserId userId,
        string name,
        string keyHash,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);

        if (expiresAtUtc.HasValue &&
            expiresAtUtc.Value <= createdAtUtc)
        {
            throw new ArgumentException(
                "Expiration must be later than creation time.",
                nameof(expiresAtUtc));
        }

        UserId = userId;

        Name = name.Trim();

        KeyHash = keyHash.Trim();

        CreatedAtUtc = createdAtUtc;

        ExpiresAtUtc = expiresAtUtc;

        LastUsedAtUtc = null;

        RevokedAtUtc = null;

        Status = ApiKeyStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new API key.
    /// </summary>
    public static ApiKey Create(
        UserId userId,
        string name,
        string keyHash,
        DateTime? expiresAtUtc = null)
    {
        return new ApiKey(
            ApiKeyId.New(),
            userId,
            name,
            keyHash,
            DateTime.UtcNow,
            expiresAtUtc);
    }

    /// <summary>
    /// Gets the owning user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the API key name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the hashed API key.
    /// Never store the raw key.
    /// </summary>
    public string KeyHash { get; }

    /// <summary>
    /// Gets when the key was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Gets when the key expires.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets when the key was last used.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; private set; }

    /// <summary>
    /// Gets when the key was revoked.
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets the current API key status.
    /// </summary>
    public ApiKeyStatus Status { get; private set; }

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
    /// Gets whether the API key is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Returns whether the API key is currently valid.
    /// </summary>
    public bool IsValid(DateTime utcNow)
    {
        return Status == ApiKeyStatus.Active &&
               (!ExpiresAtUtc.HasValue ||
                utcNow <= ExpiresAtUtc.Value);
    }    /// <summary>
         /// Records successful use of the API key.
         /// </summary>
    public void RecordUsage(DateTime usedAtUtc)
    {
        if (Status != ApiKeyStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active API key can be used.");
        }

        if (ExpiresAtUtc.HasValue &&
            usedAtUtc > ExpiresAtUtc.Value)
        {
            throw new InvalidOperationException(
                "The API key has expired.");
        }

        LastUsedAtUtc = usedAtUtc;
    }

    /// <summary>
    /// Renames the API key.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
    }

    /// <summary>
    /// Extends the expiration date.
    /// </summary>
    public void Extend(DateTime? expiresAtUtc)
    {
        if (expiresAtUtc.HasValue &&
            expiresAtUtc.Value <= CreatedAtUtc)
        {
            throw new InvalidOperationException(
                "Expiration must be later than the creation time.");
        }

        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>
    /// Revokes the API key.
    /// </summary>
    public void Revoke(DateTime revokedAtUtc)
    {
        if (Status == ApiKeyStatus.Revoked)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;

        Status = ApiKeyStatus.Revoked;
    }

    /// <summary>
    /// Marks the API key as expired.
    /// </summary>
    public void Expire()
    {
        if (Status == ApiKeyStatus.Expired)
        {
            return;
        }

        Status = ApiKeyStatus.Expired;
    }

    /// <summary>
    /// Activates the API key.
    /// </summary>
    public void Activate()
    {
        if (Status == ApiKeyStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived API key cannot be activated.");
        }

        Status = ApiKeyStatus.Active;
    }

    /// <summary>
    /// Deactivates the API key.
    /// </summary>
    public void Deactivate()
    {
        if (Status == ApiKeyStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived API key cannot be deactivated.");
        }

        Status = ApiKeyStatus.Inactive;
    }

    /// <summary>
    /// Archives the API key.
    /// </summary>
    public void Archive()
    {
        Status = ApiKeyStatus.Archived;
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
    /// Hides the API key.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the API key.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
