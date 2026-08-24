using System.IdentityModel.Tokens.Jwt;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Services;

namespace Masterdom.Core.Tests.Authentication;

public sealed class JwtTokenIssuerTests
{
    [Fact]
    public void Issue_ShouldIncludeSubjectClaimMatchingUserId()
    {
        var issuer = CreateIssuer();
        var userId = Guid.NewGuid();

        var result = issuer.Issue(userId, "alice", personId: null, ownedPropertyIds: []);

        var token = ReadToken(result.AccessToken);
        Assert.Equal(userId.ToString(), token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public void Issue_ShouldIncludePropertyScopeAndOwnedPropertyClaims_ForEachOwnedProperty()
    {
        var issuer = CreateIssuer();
        var propertyA = Guid.NewGuid();
        var propertyB = Guid.NewGuid();

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, [propertyA, propertyB]);

        var token = ReadToken(result.AccessToken);

        var scopeClaims = token.Claims
            .Where(c => c.Type == MasterdomClaimTypes.PropertyScope)
            .Select(c => Guid.Parse(c.Value))
            .ToArray();
        var ownedClaims = token.Claims
            .Where(c => c.Type == MasterdomClaimTypes.OwnedProperty)
            .Select(c => Guid.Parse(c.Value))
            .ToArray();

        Assert.Equal(2, scopeClaims.Length);
        Assert.Contains(propertyA, scopeClaims);
        Assert.Contains(propertyB, scopeClaims);
        Assert.Equal(2, ownedClaims.Length);
        Assert.Contains(propertyA, ownedClaims);
        Assert.Contains(propertyB, ownedClaims);
    }

    [Fact]
    public void Issue_WithNoOwnedProperties_ShouldEmitNoScopeClaims()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, []);

        var token = ReadToken(result.AccessToken);

        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.PropertyScope);
        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.OwnedProperty);
    }

    [Fact]
    public void Issue_ShouldNotEmitRoleOrPermissionClaims()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, []);

        var token = ReadToken(result.AccessToken);

        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.Permission);
        Assert.DoesNotContain(token.Claims, c => c.Type == "role");
    }

    [Fact]
    public void Issue_ShouldSetExpirationInTheFuture()
    {
        var issuer = CreateIssuer();
        var before = DateTime.UtcNow;

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, []);

        Assert.True(result.ExpiresAtUtc > before);
    }

    private static IJwtTokenIssuer CreateIssuer()
    {
        return new JwtTokenIssuer(new JwtTokenIssuerOptions
        {
            SigningKey = "test-signing-key-that-is-sufficiently-long",
        });
    }

    private static JwtSecurityToken ReadToken(string accessToken)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
    }
}
