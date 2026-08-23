namespace Masterdom.Abstractions.Modules.Security;

/// <summary>
/// Application layer abstraction for querying permissions associated with roles.
///
/// This abstraction is shared across the application and is implemented by the Infrastructure layer.
/// It is NOT owned by Infrastructure, but by the Application boundary.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets all permission names associated with a role.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Permission names (via RolePermission assignment) that are active.</returns>
    Task<IReadOnlyCollection<string>> GetPermissionNamesByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}
