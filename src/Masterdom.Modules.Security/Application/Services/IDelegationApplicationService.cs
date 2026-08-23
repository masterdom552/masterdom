using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Modules.Security.Application.Commands;

namespace Masterdom.Modules.Security.Application.Services;

/// <summary>
/// Application service interface for delegation management.
/// </summary>
public interface IDelegationApplicationService
{
    /// <summary>
    /// Creates a new delegation from the authenticated user to a delegatee.
    /// </summary>
    Task<DelegatedAuthority> CreateDelegationAsync(
        CreateDelegationCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an existing delegation.
    /// </summary>
    DelegatedAuthority RevokeDelegation(RevokeDelegationCommand command);

    /// <summary>
    /// Gets a delegation by ID.
    /// </summary>
    Task<DelegatedAuthority?> GetDelegationByIdAsync(Guid delegatedAuthorityId);
}
