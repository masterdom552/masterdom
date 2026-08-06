using System.Security.Claims;
using Masterdom.Core.Security;
using Masterdom.Modules.Security;
using Microsoft.AspNetCore.Http;

namespace Masterdom.Platform.Infrastructure.Tests.Property;

public sealed class CurrentUserProjectionTests
{
    [Fact]
    public void HttpContextCurrentUserAccessor_ShouldProjectClaims()
    {
        var userId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var propertyScope = Guid.NewGuid();
        var ownedPropertyId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, "claims-projection-user"),
                    new Claim(ClaimTypes.Role, MasterdomRoles.Manager),
                    new Claim(MasterdomClaimTypes.Permission, "properties.read"),
                    new Claim(MasterdomClaimTypes.PersonId, personId.ToString()),
                    new Claim(MasterdomClaimTypes.PropertyScope, propertyScope.ToString()),
                    new Claim(MasterdomClaimTypes.OwnedProperty, ownedPropertyId.ToString())
                ],
                authenticationType: "Bearer"))
        };

        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor { HttpContext = httpContext });

        var currentUser = accessor.GetCurrentUser();

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal(personId, currentUser.PersonId);
        Assert.Equal("claims-projection-user", currentUser.Username);
        Assert.True(currentUser.IsInRole(MasterdomRoles.Manager));
        Assert.True(currentUser.HasPermission("properties.read"));
        Assert.True(currentUser.HasPropertyScope(propertyScope));
        Assert.True(currentUser.OwnsProperty(ownedPropertyId));
    }
}
