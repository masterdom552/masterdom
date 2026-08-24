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
        // NOT: user has a SuperUser role claim.
        //
        // The masterdom:authority_level claim is populated exclusively by CAP-023's
        // LoginCommandHandler, which resolves it via EffectiveAuthorityResolver against
        // persisted database state at login time (see ILoginAuthorityResolver) -- it is
        // never client-supplied. A token lacking the claim (e.g. one issued before this
        // change, or for a user with no active primary role) fails closed to false.
        var authorityLevel = TryParseInt(principal.FindFirstValue(MasterdomClaimTypes.AuthorityLevel));
        var isInherentSuperUser = authorityLevel == AuthorityLevels.PrimarySuperUser;

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

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }
}
