namespace Masterdom.Core.Security;

/// <summary>
/// Resolves a user's effective authority (direct + active delegations) for
/// embedding as explicit, server-computed claims at login time. Backed by
/// the same authoritative resolution CAP-018 already trusts
/// (<see cref="EffectiveAuthorityResolver"/>) -- never a separate
/// computation.
/// </summary>
public interface ILoginAuthorityResolver
{
    /// <summary>
    /// Resolves effective authority for the given user.
    /// </summary>
    /// <param name="userId">The authenticated user.</param>
    /// <param name="directPropertyScopes">
    /// The user's own, directly-owned property scope (e.g. from
    /// <see cref="IPropertyOwnershipProvider"/>), used as the baseline that
    /// active delegations extend.
    /// </param>
    Task<LoginAuthorityClaims> ResolveAsync(
        Guid userId,
        IReadOnlyCollection<Guid> directPropertyScopes,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The resolved authority facts to embed in an issued JWT. Never carries a
/// password or password hash.
/// </summary>
public sealed record LoginAuthorityClaims(
    IReadOnlyCollection<string> RoleCodes,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<Guid> PropertyScopes,
    int? AuthorityLevel)
{
    /// <summary>
    /// The claims for a user with no active primary role assignment.
    /// Authentication still succeeds; the user is simply not yet authorized
    /// for anything role-gated.
    /// </summary>
    public static LoginAuthorityClaims None(IReadOnlyCollection<Guid> ownedPropertyScopes)
    {
        return new LoginAuthorityClaims([], [], ownedPropertyScopes, null);
    }
}
