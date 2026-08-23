using System.Collections.ObjectModel;
using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;

namespace Masterdom.Core.Security;

/// <summary>
/// Represents the computed effective authority of a user at a given point in time.
/// </summary>
public sealed class EffectiveAuthority
{
    private EffectiveAuthority(
        Guid userId,
        int effectiveLevel,
        IReadOnlyCollection<RoleId> roles,
        IReadOnlyCollection<string> permissions,
        IReadOnlyCollection<Guid> propertyScopes,
        bool isInherentSuperUser)
    {
        UserId = userId;
        EffectiveLevel = effectiveLevel;
        Roles = roles;
        Permissions = permissions;
        PropertyScopes = propertyScopes;
        IsInherentSuperUser = isInherentSuperUser;
    }

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the effective authority level (highest level available to this user).
    /// </summary>
    public int EffectiveLevel { get; }

    /// <summary>
    /// Gets all roles available to this user (direct + delegated).
    /// </summary>
    public IReadOnlyCollection<RoleId> Roles { get; }

    /// <summary>
    /// Gets all permissions available to this user (computed from roles at effective level).
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }

    /// <summary>
    /// Gets all property scopes available to this user (direct + delegated).
    /// </summary>
    public IReadOnlyCollection<Guid> PropertyScopes { get; }

    /// <summary>
    /// Gets whether this user has inherent SuperUser role (not delegated).
    /// This is used to determine if unrestricted authorization bypass applies.
    /// </summary>
    public bool IsInherentSuperUser { get; }

    /// <summary>
    /// Creates an effective authority from computed facts.
    /// </summary>
    internal static EffectiveAuthority Create(
        Guid userId,
        int effectiveLevel,
        IReadOnlyCollection<RoleId> roles,
        IReadOnlyCollection<string> permissions,
        IReadOnlyCollection<Guid> propertyScopes,
        bool isInherentSuperUser)
    {
        return new EffectiveAuthority(
            userId,
            effectiveLevel,
            roles,
            permissions,
            propertyScopes,
            isInherentSuperUser);
    }

    /// <summary>
    /// Creates an anonymous (unauthenticated) effective authority.
    /// </summary>
    internal static EffectiveAuthority Anonymous() => new(
        Guid.Empty,
        0,
        new ReadOnlyCollection<RoleId>(Array.Empty<RoleId>()),
        new ReadOnlyCollection<string>(Array.Empty<string>()),
        new ReadOnlyCollection<Guid>(Array.Empty<Guid>()),
        false);
}
