using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Security;

/// <summary>
/// Provides persistence access for <see cref="Credential"/>.
/// </summary>
public interface ICredentialRepository
{
    /// <summary>
    /// Gets the active credential for a user, if one exists.
    /// </summary>
    Task<Credential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new credential.
    /// </summary>
    void Add(Credential credential);
}
