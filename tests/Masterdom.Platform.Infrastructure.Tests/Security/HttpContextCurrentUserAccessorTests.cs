using System.Security.Claims;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves HttpContextCurrentUserAccessor's IsInherentSuperUser resolution
/// (CAP-023 Phase 2): trustworthy only because MasterdomClaimTypes.AuthorityLevel
/// is populated exclusively by server-side login-time resolution, never by
/// the client.
/// </summary>
public sealed class HttpContextCurrentUserAccessorTests
{
    [Fact]
    public void GetCurrentUser_WithPrimarySuperUserAuthorityLevelClaim_IsInherentSuperUserIsTrue()
    {
        var currentUser = Resolve(BuildPrincipal(
            new Claim(MasterdomClaimTypes.AuthorityLevel, AuthorityLevels.PrimarySuperUser.ToString())));

        Assert.True(currentUser.IsInherentSuperUser);
    }

    [Theory]
    [InlineData(AuthorityLevels.SecondarySuperUser)]
    [InlineData(AuthorityLevels.Admin)]
    [InlineData(AuthorityLevels.Tenant)]
    public void GetCurrentUser_WithNonPrimaryAuthorityLevelClaim_IsInherentSuperUserIsFalse(int level)
    {
        var currentUser = Resolve(BuildPrincipal(
            new Claim(MasterdomClaimTypes.AuthorityLevel, level.ToString())));

        Assert.False(currentUser.IsInherentSuperUser);
    }

    [Fact]
    public void GetCurrentUser_WithNoAuthorityLevelClaim_FailsClosedToFalse()
    {
        // A token issued before this change, or for a user with no primary
        // role, carries no authority_level claim at all -- must not be
        // treated as SuperUser by default.
        var currentUser = Resolve(BuildPrincipal());

        Assert.False(currentUser.IsInherentSuperUser);
    }

    [Fact]
    public void GetCurrentUser_WithSuperUserRoleClaimButNoAuthorityLevelClaim_IsInherentSuperUserIsFalse()
    {
        // A bare role-name claim alone must never establish inherent Primary
        // authority -- that was the original documented defect this fix closes.
        var currentUser = Resolve(BuildPrincipal(new Claim(ClaimTypes.Role, "SuperUser")));

        Assert.False(currentUser.IsInherentSuperUser);
        Assert.Contains("SuperUser", currentUser.Roles);
    }

    private static ClaimsPrincipal BuildPrincipal(params Claim[] extraClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "test-user"),
        };
        claims.AddRange(extraClaims);

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static CurrentUser Resolve(ClaimsPrincipal principal)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext { User = principal };

        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<ICurrentUserAccessor>();
        return currentUserAccessor.GetCurrentUser();
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
            options.UseInMemoryDatabase($"current-user-accessor-{Guid.NewGuid():N}"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Bearer:SigningKey"] = "current-user-accessor-tests-signing-key-123",
                ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                ["Authentication:Bearer:Audience"] = "masterdom-tests"
            })
            .Build();

        services.AddSecurityModule(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
