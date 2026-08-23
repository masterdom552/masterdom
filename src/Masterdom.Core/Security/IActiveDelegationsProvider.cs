using Masterdom.Core.Identity.Entities.DelegatedAuthority;

namespace Masterdom.Core.Security;

/// <summary>
/// Provides a user's active delegations, for effective-authority resolution.
/// </summary>
public interface IActiveDelegationsProvider
{
    /// <summary>
    /// Gets all active delegations for a user at the given instant.
    /// </summary>
    Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(
        Guid userId,
        DateTime utcNow);
}
