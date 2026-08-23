using Masterdom.Abstractions.Modules.Security;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core implementation of IUserRoleRepository.
/// Implements the shared Application layer abstraction for user role persistence queries.
/// </summary>
public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly MasterdomDbContext _dbContext;

    public UserRoleRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets the primary role assignment for a user.
    ///
    /// Enforces domain invariant: exactly one effective PrimaryRole must exist.
    /// </summary>
    /// <throws>InvalidOperationException if multiple effective primary roles exist (data integrity violation).</throws>
    public async Task<UserRole?> GetPrimaryRoleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = new UserId(userId);

        return await _dbContext.UserRoles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                ur => ur.UserId == userIdValue &&
                      ur.IsPrimaryRole &&
                      ur.Status == UserRoleStatus.Active,
                cancellationToken);
    }

    /// <summary>
    /// Gets all effective role assignments for a user at a specific time.
    /// </summary>
    public async Task<IReadOnlyCollection<UserRole>> GetEffectiveRolesAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = new UserId(userId);

        var userRoles = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userIdValue &&
                         ur.Status == UserRoleStatus.Active)
            .ToListAsync(cancellationToken);

        // Filter by temporal validity using the domain method
        return userRoles
            .Where(ur => ur.IsEffective(utcNow))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all primary role assignments for a user, regardless of temporal effectiveness.
    /// Used for validating the PrimaryRole invariant.
    /// </summary>
    public async Task<IReadOnlyCollection<UserRole>> GetAllPrimaryRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = new UserId(userId);

        var primaryRoles = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userIdValue &&
                         ur.IsPrimaryRole &&
                         ur.Status == UserRoleStatus.Active)
            .ToListAsync(cancellationToken);

        return primaryRoles.AsReadOnly();
    }
}
