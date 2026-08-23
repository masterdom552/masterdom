using Masterdom.Abstractions.Modules.Security;
using Masterdom.Core.Security;

namespace Masterdom.Modules.Security.Application.Services;

/// <summary>
/// Default implementation that assembles DirectAuthority from the authoritative identity model.
/// Depends on shared Application layer abstractions (IUserRoleRepository, IPermissionRepository)
/// which are implemented by the Infrastructure layer.
/// </summary>
public sealed class DefaultDirectAuthorityProvider : IDirectAuthorityProvider
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IAuthorityLevelProvider _authorityLevelProvider;

    public DefaultDirectAuthorityProvider(
        IUserRoleRepository userRoleRepository,
        IPermissionRepository permissionRepository,
        IAuthorityLevelProvider authorityLevelProvider)
    {
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _authorityLevelProvider = authorityLevelProvider ?? throw new ArgumentNullException(nameof(authorityLevelProvider));
    }

    /// <summary>
    /// Gets the direct authority for an authenticated user by:
    /// 1. Loading the user's unique effective primary role assignment
    /// 2. Verifying the role is active and temporally valid
    /// 3. Loading the role's permissions via repository
    /// 4. Constructing DirectAuthority with role ID, permissions, and property scopes
    ///
    /// Domain invariant: exactly one effective PrimaryRole per user.
    /// </summary>
    public async Task<DirectAuthority?> GetDirectAuthorityAsync(
        Guid userId,
        IReadOnlyCollection<Guid> propertyScopes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(propertyScopes);

        // 1. Load the user's primary role assignment (enforces single primary role invariant via SingleOrDefaultAsync)
        var primaryUserRole = await _userRoleRepository.GetPrimaryRoleAsync(userId, cancellationToken);

        if (primaryUserRole == null)
        {
            // User has no active primary role assignment
            return null;
        }

        var roleId = primaryUserRole.RoleId.Value;

        // 2. Load the role's permissions from persistence abstraction
        var permissions = await _permissionRepository.GetPermissionNamesByRoleAsync(
            roleId,
            cancellationToken);

        // 3. Construct and return DirectAuthority
        return new DirectAuthority(
            roleId,
            permissions,
            propertyScopes);
    }
}
