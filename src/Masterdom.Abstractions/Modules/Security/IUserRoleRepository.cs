using Masterdom.Core.Identity.Entities.UserRole;

namespace Masterdom.Abstractions.Modules.Security;

/// <summary>
/// Application layer abstraction for querying user role assignments.
///
/// This abstraction is shared across the application and is implemented by the Infrastructure layer.
/// It is NOT owned by Infrastructure, but by the Application boundary.
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>
    /// Gets the primary role assignment for a user.
    ///
    /// Domain invariant: A user must have at most one EFFECTIVE PrimaryRole at any point in time.
    /// Multiple simultaneous effective primary roles represent a data integrity violation.
    ///
    /// Note: This method DETECTS (via SingleOrDefaultAsync) but does not ENFORCE the invariant.
    /// Enforcement occurs at the persistence layer through domain operations and temporal constraints.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The unique UserRole where IsPrimaryRole=true, status is Active, and temporally effective,
    /// or null if no active primary role assignment exists.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if multiple active primary roles exist simultaneously (data integrity violation).
    /// This indicates corruption or a failure in the persistence/enforcement layer.
    /// </exception>
    Task<UserRole?> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all effective role assignments for a user at a specific time.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="utcNow">The reference time for temporal evaluation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All UserRole assignments that are active and effective at the given time.</returns>
    Task<IReadOnlyCollection<UserRole>> GetEffectiveRolesAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all primary role assignments for a user, regardless of temporal effectiveness.
    ///
    /// Used for validating the PrimaryRole temporal uniqueness invariant before making
    /// a new role primary.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// All UserRole assignments where IsPrimaryRole=true and Status=Active,
    /// regardless of their effective date ranges.
    /// </returns>
    Task<IReadOnlyCollection<UserRole>> GetAllPrimaryRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
