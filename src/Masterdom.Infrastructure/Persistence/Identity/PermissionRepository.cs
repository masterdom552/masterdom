using Masterdom.Abstractions.Modules.Security;
using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.RolePermission;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core implementation of IPermissionRepository.
/// Implements the shared Application layer abstraction for role permission persistence queries.
/// </summary>
public sealed class PermissionRepository : IPermissionRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PermissionRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets all permission names associated with a role via active RolePermission assignments.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetPermissionNamesByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .Join(
                _dbContext.Set<RolePermission>(),
                p => p.Id,
                rp => rp.PermissionId,
                (p, rp) => new { Permission = p, RolePermission = rp })
            .Where(x => x.RolePermission.RoleId == new RoleId(roleId) &&
                        x.RolePermission.Status == RolePermissionStatus.Active)
            .Select(x => x.Permission.Name.Value) // Get the permission name string
            .ToListAsync(cancellationToken);

        return permissions.AsReadOnly();
    }
}
