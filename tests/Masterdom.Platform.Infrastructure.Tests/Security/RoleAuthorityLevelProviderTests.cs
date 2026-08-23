using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Masterdom.Modules.Security.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves the PRODUCTION IAuthorityLevelProvider wiring (ADR-0010), without substituting the
/// component under test. Unlike the pre-existing Delegation integration tests (which, before this
/// package, replaced IAuthorityLevelProvider with a seeded test double), these tests resolve the
/// real, DI-registered implementation and exercise it against a real, persisted Role.
/// </summary>
public sealed class RoleAuthorityLevelProviderTests
{
    [Fact]
    public void ProductionDI_ShouldResolveRoleAuthorityLevelProviderAsIAuthorityLevelProvider()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var resolved = scope.ServiceProvider.GetService<IAuthorityLevelProvider>();

        Assert.NotNull(resolved);
        Assert.IsType<RoleAuthorityLevelProvider>(resolved);
    }

    [Theory]
    [InlineData(AuthorityLevels.PrimarySuperUser)]
    [InlineData(AuthorityLevels.SecondarySuperUser)]
    [InlineData(AuthorityLevels.Admin)]
    [InlineData(AuthorityLevels.Tenant)]
    public async Task GetAuthorityLevel_WithRealPersistedRole_ResolvesThePersistedLevel(int level)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var role = Role.Create(
            RoleCode.Create($"role-{Guid.NewGuid():N}"),
            RoleName.Create("Resolution Test Role"),
            RoleAuthorityLevel.Create(level));
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        // Resolve through the REAL, production-registered IAuthorityLevelProvider.
        var authorityLevelProvider = scope.ServiceProvider.GetRequiredService<IAuthorityLevelProvider>();

        var resolvedLevel = authorityLevelProvider.GetAuthorityLevel(role.Id.Value);

        Assert.Equal(level, resolvedLevel);
    }

    [Fact]
    public void GetAuthorityLevel_WithUnresolvableRoleId_ThrowsExplicitly()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var authorityLevelProvider = scope.ServiceProvider.GetRequiredService<IAuthorityLevelProvider>();

        var unknownRoleId = Guid.NewGuid();

        var exception = Assert.Throws<InvalidOperationException>(
            () => authorityLevelProvider.GetAuthorityLevel(unknownRoleId));

        Assert.Contains(unknownRoleId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetAuthorityLevel_AfterReclassification_ResolvesTheNewLevel()
    {
        // Confirms no caching/staleness: the production provider holds no state of its own
        // and reads the current persisted AuthorityLevel on every resolution (ADR-0010).
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var role = Role.Create(
            RoleCode.Create($"role-{Guid.NewGuid():N}"),
            RoleName.Create("Reclassification Test Role"),
            RoleAuthorityLevel.Tenant);
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        var authorityLevelProvider = scope.ServiceProvider.GetRequiredService<IAuthorityLevelProvider>();
        Assert.Equal(AuthorityLevels.Tenant, authorityLevelProvider.GetAuthorityLevel(role.Id.Value));

        role.Reclassify(RoleAuthorityLevel.SecondarySuperUser);
        await dbContext.SaveChangesAsync();

        Assert.Equal(AuthorityLevels.SecondarySuperUser, authorityLevelProvider.GetAuthorityLevel(role.Id.Value));
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
            options.UseInMemoryDatabase($"role-authority-level-{Guid.NewGuid():N}"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Bearer:SigningKey"] = "role-authority-level-tests-signing-key-1234567890",
                ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                ["Authentication:Bearer:Audience"] = "masterdom-tests"
            })
            .Build();

        services.AddSecurityModule(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
