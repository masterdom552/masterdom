using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.RolePermission;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves ILoginAuthorityResolver (CAP-023 Phase 2) against the real,
/// production-registered authority chain -- EffectiveAuthorityResolver,
/// IDirectAuthorityProvider, IDelegatedAuthorityRepository -- the same
/// components CAP-018 delegation already trusts. No test double for the
/// component under test.
/// </summary>
public sealed class LoginAuthorityResolverTests
{
    [Fact]
    public async Task ResolveAsync_ForUserWithNoPrimaryRole_ReturnsEmptyAuthority()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var resolver = scope.ServiceProvider.GetRequiredService<ILoginAuthorityResolver>();

        var claims = await resolver.ResolveAsync(Guid.NewGuid(), []);

        Assert.Empty(claims.RoleCodes);
        Assert.Empty(claims.Permissions);
        Assert.Null(claims.AuthorityLevel);
    }

    [Fact]
    public async Task ResolveAsync_ForUserWithPrimarySuperUserRole_ResolvesRoleCodeAndAuthorityLevel()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var (userId, roleId) = await SeedUserWithRoleAsync(
            dbContext, RoleAuthorityLevel.PrimarySuperUser, "SUPERUSER");

        var resolver = scope.ServiceProvider.GetRequiredService<ILoginAuthorityResolver>();
        var claims = await resolver.ResolveAsync(userId, []);

        Assert.Contains("SUPERUSER", claims.RoleCodes);
        Assert.Equal(AuthorityLevels.PrimarySuperUser, claims.AuthorityLevel);
    }

    [Fact]
    public async Task ResolveAsync_ResolvesPermissionsFromThePrimaryRole()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var (userId, roleId) = await SeedUserWithRoleAsync(
            dbContext, RoleAuthorityLevel.Admin, "ADMIN-ROLE");

        var permission = Permission.Create(
            PermissionCode.Create($"perm-{Guid.NewGuid():N}"),
            PermissionName.Create("Test Permission"));
        dbContext.Permissions.Add(permission);
        var rolePermission = RolePermission.Create(new RoleId(roleId), new PermissionId(permission.Id.Value));
        dbContext.RolePermissions.Add(rolePermission);
        await dbContext.SaveChangesAsync();

        var resolver = scope.ServiceProvider.GetRequiredService<ILoginAuthorityResolver>();
        var claims = await resolver.ResolveAsync(userId, []);

        Assert.Contains(permission.Name.Value, claims.Permissions);
    }

    [Fact]
    public async Task ResolveAsync_ReflectsRoleChange_OnNextResolution()
    {
        // Proves this is resolved fresh from the DB each call, not cached --
        // exactly the property that makes a short-lived JWT an acceptable
        // staleness boundary rather than a permanent one.
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var (userId, roleId) = await SeedUserWithRoleAsync(dbContext, RoleAuthorityLevel.Tenant, "TENANT-ROLE");

        var resolver = scope.ServiceProvider.GetRequiredService<ILoginAuthorityResolver>();
        var before = await resolver.ResolveAsync(userId, []);
        Assert.Equal(AuthorityLevels.Tenant, before.AuthorityLevel);

        var role = await dbContext.Roles.SingleAsync(r => r.Id == new RoleId(roleId));
        role.Reclassify(RoleAuthorityLevel.PrimarySuperUser);
        await dbContext.SaveChangesAsync();

        var after = await resolver.ResolveAsync(userId, []);
        Assert.Equal(AuthorityLevels.PrimarySuperUser, after.AuthorityLevel);
    }

    [Fact]
    public async Task ResolveAsync_IncludesActiveDelegatedRole_InAdditionToDirectRole()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var (userId, _) = await SeedUserWithRoleAsync(dbContext, RoleAuthorityLevel.Tenant, "TENANT-ROLE");

        var delegatedRole = Role.Create(
            RoleCode.Create($"delegated-{Guid.NewGuid():N}"),
            RoleName.Create("Delegated Manager Role"),
            RoleAuthorityLevel.Admin);
        dbContext.Roles.Add(delegatedRole);
        await dbContext.SaveChangesAsync();

        var delegation = DelegatedAuthority.Create(
            delegatorUserId: new UserId(Guid.NewGuid()),
            delegatedToUserId: new UserId(userId),
            delegatedRoleId: delegatedRole.Id,
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: DateTime.UtcNow.AddMinutes(-5),
            effectiveToUtc: DateTime.UtcNow.AddDays(1));
        dbContext.DelegatedAuthorities.Add(delegation);
        await dbContext.SaveChangesAsync();

        var resolver = scope.ServiceProvider.GetRequiredService<ILoginAuthorityResolver>();
        var claims = await resolver.ResolveAsync(userId, []);

        Assert.Equal(AuthorityLevels.Admin, claims.AuthorityLevel);
    }

    private static async Task<(Guid UserId, Guid RoleId)> SeedUserWithRoleAsync(
        MasterdomDbContext dbContext,
        RoleAuthorityLevel level,
        string roleCode)
    {
        var profile = IdentityProfile.Create(
            IdentityProfileCode.Create($"profile-{Guid.NewGuid():N}"),
            IdentityProfileType.Person);
        dbContext.IdentityProfiles.Add(profile);

        var user = User.Create(
            UserCode.Create($"user-{Guid.NewGuid():N}"),
            profile.Id,
            Username.Create($"authority-test-{Guid.NewGuid():N}"[..30]));
        dbContext.Users.Add(user);

        var role = Role.Create(RoleCode.Create(roleCode), RoleName.Create(roleCode), level);
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();

        var userRole = UserRole.Create(
            user.Id,
            role.Id,
            assignedBy: null,
            isPrimaryRole: true,
            reason: "test fixture");
        userRole.Activate();
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();

        return (user.Id.Value, role.Id.Value);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
            options.UseInMemoryDatabase($"login-authority-resolver-{Guid.NewGuid():N}"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Bearer:SigningKey"] = "login-authority-resolver-tests-signing-key-123",
                ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                ["Authentication:Bearer:Audience"] = "masterdom-tests"
            })
            .Build();

        services.AddSecurityModule(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
