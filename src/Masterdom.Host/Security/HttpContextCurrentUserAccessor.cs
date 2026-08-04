using System.Security.Claims;
using Masterdom.Core.Security;

namespace Masterdom.Host.Security;

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

        return CurrentUser.Authenticated(
            userId: ResolveUserId(principal),
            personId: ResolvePersonId(principal),
            username: principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Name),
            roles,
            permissions,
            propertyScopes,
            ownedPropertyIds);
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
