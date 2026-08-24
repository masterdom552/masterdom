using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Security;

/// <summary>
/// Provides persistence access for <see cref="PasswordReset"/>.
/// </summary>
public interface IPasswordResetRepository
{
    /// <summary>
    /// Adds a new password reset request.
    /// </summary>
    void Add(PasswordReset passwordReset);

    /// <summary>
    /// Gets the most recent pending, unexpired-or-not password reset for a
    /// user, if one exists. Used both to supersede a prior request when a
    /// new one is created, and to look up a request during redemption.
    /// </summary>
    Task<PasswordReset?> GetPendingByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically marks a pending password reset as completed, using a
    /// single conditional update (not a load-then-save round trip), so that
    /// two concurrent completion attempts against the same request cannot
    /// both succeed. Returns <c>true</c> only if this call was the one that
    /// transitioned the request from Pending to Completed.
    /// </summary>
    Task<bool> TryCompleteAsync(
        PasswordResetId id,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default);
}
