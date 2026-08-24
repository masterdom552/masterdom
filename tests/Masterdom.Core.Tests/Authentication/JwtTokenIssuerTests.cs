using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        var result = issuer.Issue(userId, "alice", personId: null, [], LoginAuthorityClaims.None([]));

        var token = ReadToken(result.AccessToken);
        Assert.Equal(userId.ToString(), token.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public void Issue_ShouldEmitOwnedPropertyClaims_FromOwnedPropertyIds()
    {
        var issuer = CreateIssuer();
        var propertyA = Guid.NewGuid();
        var propertyB = Guid.NewGuid();

        var result = issuer.Issue(
            Guid.NewGuid(), "alice", personId: null,
            [propertyA, propertyB],
            LoginAuthorityClaims.None([propertyA, propertyB]));

        var token = ReadToken(result.AccessToken);

        var ownedClaims = token.Claims
            .Where(c => c.Type == MasterdomClaimTypes.OwnedProperty)
            .Select(c => Guid.Parse(c.Value))
            .ToArray();

        Assert.Equal(2, ownedClaims.Length);
        Assert.Contains(propertyA, ownedClaims);
        Assert.Contains(propertyB, ownedClaims);
    }

    [Fact]
    public void Issue_ShouldEmitPropertyScopeClaims_FromAuthorityPropertyScopes_NotJustOwnedProperties()
    {
        var issuer = CreateIssuer();
        var ownedProperty = Guid.NewGuid();
        var delegatedProperty = Guid.NewGuid();

        var result = issuer.Issue(
            Guid.NewGuid(), "alice", personId: null,
            ownedPropertyIds: [ownedProperty],
            authority: new LoginAuthorityClaims([], [], [ownedProperty, delegatedProperty], AuthorityLevel: null));

        var token = ReadToken(result.AccessToken);

        var scopeClaims = token.Claims
            .Where(c => c.Type == MasterdomClaimTypes.PropertyScope)
            .Select(c => Guid.Parse(c.Value))
            .ToArray();

        Assert.Equal(2, scopeClaims.Length);
        Assert.Contains(ownedProperty, scopeClaims);
        Assert.Contains(delegatedProperty, scopeClaims);
    }

    [Fact]
    public void Issue_WithNoProperties_ShouldEmitNoScopeOrOwnedClaims()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, [], LoginAuthorityClaims.None([]));

        var token = ReadToken(result.AccessToken);

        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.PropertyScope);
        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.OwnedProperty);
    }

    [Fact]
    public void Issue_WithNoAuthority_ShouldEmitNoRolePermissionOrAuthorityLevelClaims()
    {
        var issuer = CreateIssuer();

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, [], LoginAuthorityClaims.None([]));

        var token = ReadToken(result.AccessToken);

        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.Permission);
        Assert.DoesNotContain(token.Claims, c => c.Type == ClaimTypes.Role);
        Assert.DoesNotContain(token.Claims, c => c.Type == MasterdomClaimTypes.AuthorityLevel);
    }

    [Fact]
    public void Issue_WithResolvedAuthority_ShouldEmitServerComputedRolePermissionAndAuthorityLevelClaims()
    {
        var issuer = CreateIssuer();
        var authority = new LoginAuthorityClaims(
            RoleCodes: ["SUPERUSER"],
            Permissions: ["property:read"],
            PropertyScopes: [],
            AuthorityLevel: AuthorityLevels.PrimarySuperUser);

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, [], authority);

        var token = ReadToken(result.AccessToken);

        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "SUPERUSER");
        Assert.Contains(token.Claims, c => c.Type == MasterdomClaimTypes.Permission && c.Value == "property:read");
        Assert.Contains(
            token.Claims,
            c => c.Type == MasterdomClaimTypes.AuthorityLevel
                && c.Value == AuthorityLevels.PrimarySuperUser.ToString());
    }

    [Fact]
    public void Issue_ShouldSetExpirationInTheFuture()
    {
        var issuer = CreateIssuer();
        var before = DateTime.UtcNow;

        var result = issuer.Issue(Guid.NewGuid(), "alice", personId: null, [], LoginAuthorityClaims.None([]));

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
