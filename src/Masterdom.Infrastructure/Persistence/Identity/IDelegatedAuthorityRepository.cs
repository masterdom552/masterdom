using Masterdom.Core.Identity.Entities.DelegatedAuthority;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// Repository interface for DelegatedAuthority.
/// </summary>
public interface IDelegatedAuthorityRepository
{
    /// <summary>
    /// Gets a delegation by ID.
    /// </summary>
    Task<DelegatedAuthority?> GetByIdAsync(DelegatedAuthorityId id);

    /// <summary>
    /// Gets all active delegations for a user at the given time.
    /// </summary>
    Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(
        Guid delegatedToUserId,
        DateTime utcNow);

    /// <summary>
    /// Gets all delegations created by a user.
    /// </summary>
    Task<IReadOnlyCollection<DelegatedAuthority>> GetDelegationsByDelegatorAsync(Guid delegatorUserId);

    /// <summary>
    /// Adds a new delegation.
    /// </summary>
    void Add(DelegatedAuthority delegation);

    /// <summary>
    /// Updates an existing delegation.
    /// </summary>
    void Update(DelegatedAuthority delegation);
}
