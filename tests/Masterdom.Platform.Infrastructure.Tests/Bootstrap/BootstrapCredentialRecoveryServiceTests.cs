using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Security;
using Masterdom.Host.Bootstrap;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Identity;
using Masterdom.Infrastructure.Persistence.People;
using Masterdom.Modules.Authentication.Application.Commands;
using Masterdom.Modules.Authentication.Application.Handlers;
using Masterdom.Modules.Authentication.Application.Services;
using Masterdom.Modules.People.Domain.Repositories;
using Masterdom.Modules.Security.Domain.Repositories;
using Masterdom.Modules.Security.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Infrastructure.Tests.Bootstrap;

/// <summary>
/// Tests for the CAP-001 Phase 2 Bootstrap Credential Recovery service
/// against an EF Core InMemory-backed <see cref="MasterdomDbContext"/> -- no
/// HTTP host, no WebApplicationFactory, mirroring
/// <see cref="BootstrapProvisioningServiceTests"/>'s established style.
/// </summary>
public sealed class BootstrapCredentialRecoveryServiceTests
{
    private const string ValidRecoverySecret = "a-sufficiently-long-recovery-secret";

    private static readonly BootstrapRequest BootstrapSeed = new(
        Username: "bootstrap-admin",
        Password: "original-bootstrap-password-1",
        FirstName: "System",
        LastName: "Administrator");

    [Fact]
    public async Task RecoverAsync_WithCorrectSecretAndPrimarySuperUser_ChangesOnlyThatCredential()
    {
        using var dbContext = CreateDbContext();
        var bootstrapResult = await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ValidRecoverySecret));

        Assert.True(result.Success);
        Assert.Equal(bootstrapResult.UserId, result.UserId);

        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.Roles.CountAsync());
        Assert.Equal(1, await dbContext.UserRoles.CountAsync());
        Assert.Equal(1, await dbContext.Credentials.CountAsync());
        Assert.Equal(0, await dbContext.Properties.CountAsync());
    }

    [Fact]
    public async Task RecoverAsync_OldPasswordNoLongerAuthenticates_NewPasswordDoes()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ValidRecoverySecret));

        var credential = await dbContext.Credentials.SingleAsync();
        var hasher = new PasswordHasher();
        Assert.False(hasher.Verify(credential.PasswordHash, BootstrapSeed.Password));
        Assert.True(hasher.Verify(credential.PasswordHash, "brand-new-recovered-password"));
    }

    [Fact]
    public async Task RecoverAsync_RecoveredCredential_IsUsableByRealLoginFlow()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        var recoveryResult = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ValidRecoverySecret));
        Assert.True(recoveryResult.Success);

        var loginHandler = CreateLoginHandler(dbContext);

        var loginResult = await loginHandler.HandleAsync(
            new LoginCommand(BootstrapSeed.Username, "brand-new-recovered-password"));

        Assert.True(loginResult.IsSuccess);
        Assert.NotNull(loginResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Value!.AccessToken));

        var oldPasswordLogin = await loginHandler.HandleAsync(
            new LoginCommand(BootstrapSeed.Username, BootstrapSeed.Password));
        Assert.False(oldPasswordLogin.IsSuccess);
    }

    [Fact]
    public async Task RecoverAsync_AfterRecovery_BootstrapIdempotencyGuardStillRejectsRerun()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ValidRecoverySecret));

        var bootstrapService = new BootstrapProvisioningService(
            dbContext,
            new PersonRepository(dbContext),
            new RoleRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher());

        var rerun = await bootstrapService.RunAsync(BootstrapSeed with { Username = "another-admin" });

        Assert.False(rerun.Success);
        Assert.Contains("already", rerun.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.Roles.CountAsync());
    }

    [Fact]
    public async Task RecoverAsync_WithMissingRecoverySecret_FailsAndLeavesCredentialUnchanged()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);
        var originalHash = (await dbContext.Credentials.SingleAsync()).PasswordHash;

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ""));

        Assert.False(result.Success);
        Assert.Equal(originalHash, (await dbContext.Credentials.SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task RecoverAsync_WithTooShortRecoverySecret_FailsAndLeavesCredentialUnchanged()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);
        var originalHash = (await dbContext.Credentials.SingleAsync()).PasswordHash;

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", "too-short"));

        Assert.False(result.Success);
        Assert.Equal(originalHash, (await dbContext.Credentials.SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task RecoverAsync_WithUnknownUsername_FailsAndMutatesNothing()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);
        var originalHash = (await dbContext.Credentials.SingleAsync()).PasswordHash;

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            "no-such-user", "brand-new-recovered-password", ValidRecoverySecret));

        Assert.False(result.Success);
        Assert.Equal(1, await dbContext.Credentials.CountAsync());
        Assert.Equal(originalHash, (await dbContext.Credentials.SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task RecoverAsync_WithUsernameThatIsNotPrimarySuperUser_FailsAndMutatesNothing()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var ordinaryUser = await CreateOrdinaryUserWithCredentialAsync(dbContext, "ordinary-user", "ordinary-password-1");
        var service = CreateService(dbContext);
        var ordinaryCredentialHash = (await new CredentialRepository(dbContext)
            .GetByUserIdAsync(ordinaryUser.Id))!.PasswordHash;

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            "ordinary-user", "brand-new-recovered-password", ValidRecoverySecret));

        Assert.False(result.Success);
        var stillOrdinaryCredential = await new CredentialRepository(dbContext).GetByUserIdAsync(ordinaryUser.Id);
        Assert.Equal(ordinaryCredentialHash, stillOrdinaryCredential!.PasswordHash);
    }

    [Fact]
    public async Task RecoverAsync_WithPrimarySuperUserHavingNoCredential_FailsAndCreatesNoCredential()
    {
        using var dbContext = CreateDbContext();
        await CreatePrimarySuperUserWithoutCredentialAsync(dbContext, "credential-less-admin");
        var service = CreateService(dbContext);

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            "credential-less-admin", "brand-new-recovered-password", ValidRecoverySecret));

        Assert.False(result.Success);
        Assert.Equal(0, await dbContext.Credentials.CountAsync());
    }

    [Fact]
    public async Task RecoverAsync_WithTooShortNewPassword_FailsAndLeavesCredentialUnchanged()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);
        var originalHash = (await dbContext.Credentials.SingleAsync()).PasswordHash;

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "short", ValidRecoverySecret));

        Assert.False(result.Success);
        Assert.Equal(originalHash, (await dbContext.Credentials.SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task RecoverAsync_InvokedTwiceIntentionally_SucceedsBothTimes()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        var first = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "first-recovered-password", ValidRecoverySecret));
        var second = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "second-recovered-password", ValidRecoverySecret));

        Assert.True(first.Success);
        Assert.True(second.Success);

        var credential = await dbContext.Credentials.SingleAsync();
        var hasher = new PasswordHasher();
        Assert.True(hasher.Verify(credential.PasswordHash, "second-recovered-password"));
        Assert.False(hasher.Verify(credential.PasswordHash, "first-recovered-password"));
    }

    [Fact]
    public async Task RecoverAsync_ResultShape_NeverExposesSecretPasswordOrHash()
    {
        using var dbContext = CreateDbContext();
        await SeedBootstrapIdentityAsync(dbContext);
        var service = CreateService(dbContext);

        var result = await service.RecoverAsync(new BootstrapCredentialRecoveryRequest(
            BootstrapSeed.Username, "brand-new-recovered-password", ValidRecoverySecret));

        var properties = typeof(BootstrapCredentialRecoveryResult).GetProperties().Select(p => p.Name).ToArray();
        Assert.Equal(["Success", "Message", "UserId"], properties);
        Assert.DoesNotContain("brand-new-recovered-password", result.Message);
        Assert.DoesNotContain(ValidRecoverySecret, result.Message);

        var credential = await dbContext.Credentials.SingleAsync();
        Assert.DoesNotContain(credential.PasswordHash, result.Message);
    }

    private static MasterdomDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MasterdomDbContext(options);
    }

    private static BootstrapCredentialRecoveryService CreateService(MasterdomDbContext dbContext)
    {
        return new BootstrapCredentialRecoveryService(
            dbContext,
            new UserRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher());
    }

    private static LoginCommandHandler CreateLoginHandler(MasterdomDbContext dbContext)
    {
        return new LoginCommandHandler(
            new UserRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher(),
            new NoOwnedPropertiesProvider(),
            new NoAuthorityResolver(),
            new JwtTokenIssuer(new JwtTokenIssuerOptions
            {
                SigningKey = "test-signing-key-that-is-sufficiently-long",
            }));
    }

    private static async Task<BootstrapResult> SeedBootstrapIdentityAsync(MasterdomDbContext dbContext)
    {
        var bootstrapService = new BootstrapProvisioningService(
            dbContext,
            new PersonRepository(dbContext),
            new RoleRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher());

        var result = await bootstrapService.RunAsync(BootstrapSeed);
        Assert.True(result.Success);

        return result;
    }

    private static async Task<User> CreateOrdinaryUserWithCredentialAsync(
        MasterdomDbContext dbContext,
        string username,
        string password)
    {
        var identityProfile = IdentityProfile.Create(
            IdentityProfileCode.Create($"ORDINARY-{Guid.NewGuid():N}"),
            IdentityProfileType.Person);
        dbContext.IdentityProfiles.Add(identityProfile);

        var user = User.Create(
            UserCode.Create($"ORDINARY-{Guid.NewGuid():N}"),
            identityProfile.Id,
            Username.Create(username));
        dbContext.Users.Add(user);

        var credential = Credential.Create(user.Id, new PasswordHasher().Hash(password));
        dbContext.Credentials.Add(credential);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private static async Task<User> CreatePrimarySuperUserWithoutCredentialAsync(
        MasterdomDbContext dbContext,
        string username)
    {
        var identityProfile = IdentityProfile.Create(
            IdentityProfileCode.Create($"NOCRED-{Guid.NewGuid():N}"),
            IdentityProfileType.Person);
        dbContext.IdentityProfiles.Add(identityProfile);

        var user = User.Create(
            UserCode.Create($"NOCRED-{Guid.NewGuid():N}"),
            identityProfile.Id,
            Username.Create(username));
        dbContext.Users.Add(user);

        var role = Role.Create(
            RoleCode.Create("SUPERUSER-NOCRED"),
            RoleName.Create("Super User (No Credential)"),
            RoleAuthorityLevel.PrimarySuperUser);
        dbContext.Roles.Add(role);

        var userRole = UserRole.Create(
            user.Id,
            role.Id,
            assignedBy: null,
            isPrimaryRole: true,
            reason: "Test fixture: PrimarySuperUser without a credential");
        userRole.Activate();
        dbContext.UserRoles.Add(userRole);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private sealed class NoOwnedPropertiesProvider : IPropertyOwnershipProvider
    {
        public Task<IReadOnlyCollection<Guid>> GetOwnedPropertyIdsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>([]);
        }
    }

    private sealed class NoAuthorityResolver : ILoginAuthorityResolver
    {
        public Task<LoginAuthorityClaims> ResolveAsync(
            Guid userId,
            IReadOnlyCollection<Guid> directPropertyScopes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(LoginAuthorityClaims.None(directPropertyScopes));
        }
    }
}
