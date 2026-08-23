using System.Collections.ObjectModel;
using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Security;

/// <summary>
/// Domain service for computing effective authority.
///
/// This is a pure calculation over supplied facts, with no persistence dependencies.
/// It receives:
/// - Direct authority facts (from UserRole, Permission, PropertyOwner)
/// - Delegated authority facts (from DelegatedAuthority records)
/// - Authority level configuration (role → level mapping)
/// - Current time (for temporal evaluation)
///
/// And computes:
/// - Effective authority level
/// - Effective roles (direct + delegated, respecting level)
/// - Effective permissions (computed from effective roles)
/// - Effective property scopes (direct + delegated, respecting scope)
/// - Whether user has inherent SuperUser role
/// </summary>
public sealed class EffectiveAuthorityResolver
{
    private readonly IAuthorityLevelProvider _authorityLevelProvider;

    public EffectiveAuthorityResolver(IAuthorityLevelProvider authorityLevelProvider)
    {
        _authorityLevelProvider = authorityLevelProvider ?? throw new ArgumentNullException(nameof(authorityLevelProvider));
    }

    /// <summary>
    /// Computes effective authority from supplied facts.
    /// </summary>
    public EffectiveAuthority Resolve(
        Guid userId,
        DirectAuthority directAuthority,
        IReadOnlyCollection<DelegatedAuthority> activeDelegations,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(directAuthority);
        ArgumentNullException.ThrowIfNull(activeDelegations);

        // Compute effective level
        var directLevel = _authorityLevelProvider.GetAuthorityLevel(directAuthority.PrimaryRoleId);
        var delegatedLevels = activeDelegations
            .Where(d => IsEffectiveDelegation(d, utcNow))
            .Select(d => _authorityLevelProvider.GetAuthorityLevel(d.DelegatedRoleId.Value))
            .ToList();

        var effectiveLevel = directLevel;
        if (delegatedLevels.Count > 0)
        {
            // If any delegated level equals or exceeds direct level, use max of all
            var maxDelegatedLevel = delegatedLevels.Max();
            effectiveLevel = Math.Max(directLevel, maxDelegatedLevel);
        }

        // Determine if user has inherent SuperUser
        var isInherentSuperUser = directLevel == AuthorityLevels.PrimarySuperUser;

        // Collect all effective roles
        var roles = new HashSet<RoleId> { RoleId.From(directAuthority.PrimaryRoleId) };
        foreach (var delegation in activeDelegations.Where(d => IsEffectiveDelegation(d, utcNow)))
        {
            roles.Add(delegation.DelegatedRoleId);
        }

        // Collect all effective property scopes
        var propertyScopes = new HashSet<Guid>(directAuthority.PropertyScopes);
        foreach (var delegation in activeDelegations.Where(d => IsEffectiveDelegation(d, utcNow)))
        {
            if (delegation.Scope.PropertyIds != null)
            {
                foreach (var propertyId in delegation.Scope.PropertyIds)
                {
                    propertyScopes.Add(propertyId);
                }
            }
            else
            {
                // Unrestricted delegation includes all delegator's scopes
                foreach (var propertyId in directAuthority.PropertyScopes)
                {
                    propertyScopes.Add(propertyId);
                }
            }
        }

        // Compute effective permissions from direct authority only
        // (delegated roles grant no additional permissions beyond their level)
        var permissions = directAuthority.Permissions;

        return EffectiveAuthority.Create(
            userId,
            effectiveLevel,
            new ReadOnlyCollection<RoleId>(roles.ToList()),
            permissions,
            new ReadOnlyCollection<Guid>(propertyScopes.ToList()),
            isInherentSuperUser);
    }

    private static bool IsEffectiveDelegation(DelegatedAuthority delegation, DateTime utcNow)
    {
        if (delegation.Status == DelegatedAuthorityStatus.Revoked)
            return false;

        if (utcNow < delegation.EffectiveFromUtc)
            return false;

        if (delegation.EffectiveToUtc.HasValue && utcNow > delegation.EffectiveToUtc.Value)
            return false;

        return true;
    }
}

/// <summary>
/// Direct authority facts for a user.
/// </summary>
public sealed class DirectAuthority
{
    public DirectAuthority(
        Guid roleId,
        IReadOnlyCollection<string> permissions,
        IReadOnlyCollection<Guid> propertyScopes)
    {
        PrimaryRoleId = roleId;
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        PropertyScopes = propertyScopes ?? throw new ArgumentNullException(nameof(propertyScopes));
    }

    /// <summary>
    /// The user's primary (direct) role.
    /// </summary>
    public Guid PrimaryRoleId { get; }

    /// <summary>
    /// Permissions granted by the direct role.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }

    /// <summary>
    /// Property scopes available to the user directly.
    /// </summary>
    public IReadOnlyCollection<Guid> PropertyScopes { get; }
}
