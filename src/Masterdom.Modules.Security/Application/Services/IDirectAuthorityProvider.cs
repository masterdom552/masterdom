using Masterdom.Core.Security;

namespace Masterdom.Modules.Security.Application.Services;

/// <summary>
/// Provides the direct authority facts for an authenticated user.
/// </summary>
public interface IDirectAuthorityProvider
{
    /// <summary>
    /// Gets the direct authority for an authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user ID.</param>
    /// <param name="propertyScopes">The user's authorized property scopes (from claims).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// DirectAuthority containing the user's primary role and permissions.
    /// Returns null if the user has no active primary role assignment.
    /// </returns>
    Task<DirectAuthority?> GetDirectAuthorityAsync(
        Guid userId,
        IReadOnlyCollection<Guid> propertyScopes,
        CancellationToken cancellationToken = default);
}
