using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserSession;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.RefreshToken;

/// <summary>
/// Represents a refresh token used to obtain new access tokens.
/// </summary>
public sealed class RefreshToken : AggregateRoot<RefreshTokenId>
{
    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        UserSessionId userSessionId,
        string tokenHash,
        DateTime issuedAtUtc,
        DateTime expiresAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(userSessionId);

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (expiresAtUtc <= issuedAtUtc)
        {
            throw new ArgumentException(
                "Expiration must be later than issue time.",
                nameof(expiresAtUtc));
        }

        UserId = userId;

        UserSessionId = userSessionId;

        TokenHash = tokenHash.Trim();

        IssuedAtUtc = issuedAtUtc;

        ExpiresAtUtc = expiresAtUtc;

        Status = RefreshTokenStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    public static RefreshToken Create(
        UserId userId,
        UserSessionId userSessionId,
        string tokenHash,
        TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;

        return new RefreshToken(
            RefreshTokenId.New(),
            userId,
            userSessionId,
            tokenHash,
            now,
            now.Add(lifetime));
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the owning session.
    /// </summary>
    public UserSessionId UserSessionId { get; }

    /// <summary>
    /// Gets the hashed refresh token.
    /// Never store the raw token.
    /// </summary>
    public string TokenHash { get; }

    /// <summary>
    /// Gets when the token was issued.
    /// </summary>
    public DateTime IssuedAtUtc { get; }

    /// <summary>
    /// Gets when the token expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets when the token was consumed.
    /// </summary>
    public DateTime? UsedAtUtc { get; private set; }

    /// <summary>
    /// Gets when the token was revoked.
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets the token status.
    /// </summary>
    public RefreshTokenStatus Status { get; private set; }

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
    /// Gets whether the token is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Returns whether the token is currently usable.
    /// </summary>
    public bool IsValid(DateTime utcNow)
    {
        return Status == RefreshTokenStatus.Active &&
               utcNow <= ExpiresAtUtc;
    }    /// <summary>
         /// Marks the refresh token as used.
         /// </summary>
    public void MarkAsUsed(DateTime usedAtUtc)
    {
        if (Status != RefreshTokenStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active refresh token can be used.");
        }

        if (usedAtUtc > ExpiresAtUtc)
        {
            throw new InvalidOperationException(
                "The refresh token has already expired.");
        }

        UsedAtUtc = usedAtUtc;

        Status = RefreshTokenStatus.Used;
    }

    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    public void Revoke(DateTime revokedAtUtc)
    {
        if (Status == RefreshTokenStatus.Revoked)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;

        Status = RefreshTokenStatus.Revoked;
    }

    /// <summary>
    /// Expires the refresh token.
    /// </summary>
    public void Expire()
    {
        if (Status == RefreshTokenStatus.Expired)
        {
            return;
        }

        Status = RefreshTokenStatus.Expired;
    }

    /// <summary>
    /// Extends the token expiration.
    /// </summary>
    public void Extend(DateTime expiresAtUtc)
    {
        if (expiresAtUtc <= IssuedAtUtc)
        {
            throw new InvalidOperationException(
                "Expiration must be later than the issue time.");
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
    /// Changes the remarks.
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
    /// Hides the refresh token.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the refresh token.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
