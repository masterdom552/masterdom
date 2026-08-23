using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Security;
using Masterdom.Modules.Security.Domain.Repositories;

namespace Masterdom.Modules.Security.Infrastructure;

/// <summary>
/// Production implementation of <see cref="IAuthorityLevelProvider"/>.
///
/// Resolves a role's authority level by loading the authoritative, persisted
/// <see cref="Role.AuthorityLevel"/> classification through <see cref="IRoleRepository"/>.
/// See ADR-0010.
///
/// This implementation is request-scoped and database-backed. It holds no cache and
/// requires no startup population: <see cref="Role.AuthorityLevel"/> is the single
/// authoritative source, read fresh on every resolution.
/// </summary>
public sealed class RoleAuthorityLevelProvider : IAuthorityLevelProvider
{
    private readonly IRoleRepository _roleRepository;

    public RoleAuthorityLevelProvider(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    /// <summary>
    /// Resolves the authority level for the given role.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="roleId"/> does not resolve to a persisted <see cref="Role"/>.
    /// This is a fail-explicit boundary: an unresolvable role must never silently be
    /// treated as a valid, low-privilege classification (see ADR-0010).
    /// </exception>
    public int GetAuthorityLevel(Guid roleId)
    {
        var role = _roleRepository.GetById(RoleId.From(roleId));

        if (role is null)
        {
            throw new InvalidOperationException(
                $"Role '{roleId}' could not be resolved. Authority level cannot be determined for an unknown role.");
        }

        return role.AuthorityLevel.Value;
    }
}
