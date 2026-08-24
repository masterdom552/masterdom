using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Credential;

/// <summary>
/// Represents a user's password credential.
///
/// Exactly one active credential exists per user. The stored value is always
/// a password hash -- the raw password is never persisted.
/// </summary>
public sealed class Credential : AggregateRoot<CredentialId>
{
    private Credential(
        CredentialId id,
        UserId userId,
        string passwordHash,
        DateTime createdAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        UserId = userId;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
        ChangedAtUtc = createdAtUtc;
        Status = CredentialStatus.Active;
    }

    /// <summary>
    /// Creates a new credential for a user.
    /// </summary>
    public static Credential Create(UserId userId, string passwordHash)
    {
        var now = DateTime.UtcNow;

        return new Credential(
            CredentialId.New(),
            userId,
            passwordHash,
            now);
    }

    /// <summary>
    /// Gets the user this credential belongs to.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the stored password hash. Never the raw password.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets when this credential was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Gets when this credential's password hash was last changed.
    /// </summary>
    public DateTime ChangedAtUtc { get; private set; }

    /// <summary>
    /// Gets the credential's lifecycle status.
    /// </summary>
    public CredentialStatus Status { get; private set; }

    /// <summary>
    /// Replaces the stored password hash.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);

        PasswordHash = newPasswordHash;
        ChangedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes this credential.
    /// </summary>
    public void Revoke()
    {
        if (Status == CredentialStatus.Revoked)
        {
            throw new InvalidOperationException(
                "Credential is already revoked.");
        }

        Status = CredentialStatus.Revoked;
    }
}
