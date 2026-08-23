using System.Security.Claims;
using Masterdom.Core.Security;
using Microsoft.AspNetCore.Http;

namespace Masterdom.Modules.Security;

internal sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public CurrentUser GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return CurrentUser.Anonymous;
        }

        var roles = principal.Claims
            .Where(x => x.Type == ClaimTypes.Role || string.Equals(x.Type, "role", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = principal.FindAll(MasterdomClaimTypes.Permission)
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var propertyScopes = principal.FindAll(MasterdomClaimTypes.PropertyScope)
            .Select(x => TryParseGuid(x.Value))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        var ownedPropertyIds = principal.FindAll(MasterdomClaimTypes.OwnedProperty)
            .Select(x => TryParseGuid(x.Value))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        // SECURITY CRITICAL:
        // IsInherentSuperUser must represent: user possesses inherent Primary authority
        // NOT: user has a SuperUser role claim
        //
        // These are NOT equivalent:
        // - SuperUser role claim ≠ inherent Primary authority
        // - Delegated SuperUser ≠ inherent Primary authority
        //
        // Current JWT cannot safely establish inherent Primary authority:
        // - Role claims are orthogonal to authority level
        // - Delegations are in database, not JWT
        // - We cannot distinguish direct Primary from Secondary with SuperUser role
        //
        // FAIL CLOSED: Cannot establish inherent Primary from JWT claims alone.
        // Without authoritative evidence (e.g., explicit authority-level claim,
        // database verification, or future authentication service confirmation),
        // default to false.
        //
        // Production: Authentication service must verify user's primary authority
        // in database BEFORE issuing token, then include explicit authority evidence
        // (e.g., "authority_level": "primary") in JWT if needed.
        //
        // This deferred to Application Security implementation (future).
        var isInherentSuperUser = false;

        return CurrentUser.Authenticated(
            userId: ResolveUserId(principal),
            personId: ResolvePersonId(principal),
            username: principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Name),
            roles,
            permissions,
            propertyScopes,
            ownedPropertyIds,
            isInherentSuperUser);
    }

    private static Guid? ResolveUserId(ClaimsPrincipal principal)
    {
        return TryParseGuid(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub"));
    }

    private static Guid? ResolvePersonId(ClaimsPrincipal principal)
    {
        return TryParseGuid(principal.FindFirstValue(MasterdomClaimTypes.PersonId));
    }

    private static Guid? TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out var result) ? result : null;
    }
}
