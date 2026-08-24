using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Security;

/// <summary>
/// Provides read access to <see cref="User"/> by its authentication-relevant
/// natural key.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by username, if one exists.
    /// </summary>
    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the person linked to a user's identity profile, if one exists.
    /// </summary>
    Task<Guid?> GetLinkedPersonIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
