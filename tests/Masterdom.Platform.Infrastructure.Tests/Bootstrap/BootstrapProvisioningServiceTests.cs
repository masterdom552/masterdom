using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
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
/// Tests for the CAP-001 Bootstrap Provisioning service against an EF Core
/// InMemory-backed <see cref="MasterdomDbContext"/> -- no HTTP host, no
/// WebApplicationFactory, avoiding the separate, pre-existing connection-
/// string test-infrastructure limitation entirely.
/// </summary>
public sealed class BootstrapProvisioningServiceTests
{
    private static readonly BootstrapRequest ValidRequest = new(
        Username: "bootstrap-admin",
        Password: "correct-bootstrap-password-1",
        FirstName: "System",
        LastName: "Administrator");

    [Fact]
    public async Task RunAsync_OnFreshDatabase_CreatesCompleteIdentityGraph()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RunAsync(ValidRequest);

        Assert.True(result.Success);
        Assert.NotNull(result.UserId);

        Assert.Equal(1, await dbContext.Persons.CountAsync());
        Assert.Equal(1, await dbContext.IdentityProfiles.CountAsync());
        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.Credentials.CountAsync());
        Assert.Equal(1, await dbContext.Roles.CountAsync());
        Assert.Equal(1, await dbContext.UserRoles.CountAsync());

        var role = await dbContext.Roles.SingleAsync();
        Assert.Equal(RoleAuthorityLevel.PrimarySuperUser, role.AuthorityLevel);

        var userRole = await dbContext.UserRoles.SingleAsync();
        Assert.True(userRole.IsPrimaryRole);
        Assert.Equal(role.Id, userRole.RoleId);
        Assert.Equal(new UserId(result.UserId!.Value), userRole.UserId);
    }

    [Fact]
    public async Task RunAsync_PasswordIsStoredOnlyThroughPasswordHasher()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await service.RunAsync(ValidRequest);

        var credential = await dbContext.Credentials.SingleAsync();
        Assert.NotEqual(ValidRequest.Password, credential.PasswordHash);

        var hasher = new PasswordHasher();
        Assert.True(hasher.Verify(credential.PasswordHash, ValidRequest.Password));
    }

    [Fact]
    public async Task RunAsync_ProvisionedCredential_IsUsableByRealLoginFlow()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var bootstrapResult = await service.RunAsync(ValidRequest);
        Assert.True(bootstrapResult.Success);

        var loginHandler = new LoginCommandHandler(
            new UserRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher(),
            new NoOwnedPropertiesProvider(),
            new NoAuthorityResolver(),
            new JwtTokenIssuer(new JwtTokenIssuerOptions
            {
                SigningKey = "test-signing-key-that-is-sufficiently-long",
            }));

        var loginResult = await loginHandler.HandleAsync(
            new LoginCommand(ValidRequest.Username, ValidRequest.Password));

        Assert.True(loginResult.IsSuccess);
        Assert.NotNull(loginResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Value!.AccessToken));
    }

    [Fact]
    public async Task RunAsync_WithWrongPasswordAfterBootstrap_LoginFailsGenerically()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);
        await service.RunAsync(ValidRequest);

        var loginHandler = new LoginCommandHandler(
            new UserRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher(),
            new NoOwnedPropertiesProvider(),
            new NoAuthorityResolver(),
            new JwtTokenIssuer(new JwtTokenIssuerOptions
            {
                SigningKey = "test-signing-key-that-is-sufficiently-long",
            }));

        var loginResult = await loginHandler.HandleAsync(
            new LoginCommand(ValidRequest.Username, "wrong-password"));

        Assert.False(loginResult.IsSuccess);
        Assert.Equal("unauthorized", loginResult.ErrorCode);
    }

    [Fact]
    public async Task RunAsync_WhenPrimarySuperUserRoleAlreadyExists_RejectsSafely()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var first = await service.RunAsync(ValidRequest);
        Assert.True(first.Success);

        var second = await service.RunAsync(ValidRequest with { Username = "second-admin" });

        Assert.False(second.Success);
        Assert.Contains("already", second.Message, StringComparison.OrdinalIgnoreCase);

        // No second administrator, no duplicate roles/credentials were created.
        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.Roles.CountAsync());
        Assert.Equal(1, await dbContext.Credentials.CountAsync());
        Assert.Equal(1, await dbContext.UserRoles.CountAsync());
    }

    [Fact]
    public async Task RunAsync_WithMissingPassword_FailsAndPersistsNothing()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RunAsync(ValidRequest with { Password = "" });

        Assert.False(result.Success);
        Assert.Equal(0, await dbContext.Users.CountAsync());
        Assert.Equal(0, await dbContext.Credentials.CountAsync());
        Assert.Equal(0, await dbContext.Roles.CountAsync());
        Assert.Equal(0, await dbContext.IdentityProfiles.CountAsync());
        Assert.Equal(0, await dbContext.Persons.CountAsync());
    }

    [Fact]
    public async Task RunAsync_WithInvalidUsername_FailsBeforePersistingAnything()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        // "ab" is shorter than Username's own minimum-length invariant (3 chars),
        // so Username.Create throws before any repository Add call is reached --
        // proving the request is validated before persistence begins.
        var result = await service.RunAsync(ValidRequest with { Username = "ab" });

        Assert.False(result.Success);
        Assert.Equal(0, await dbContext.Users.CountAsync());
        Assert.Equal(0, await dbContext.Credentials.CountAsync());
        Assert.Equal(0, await dbContext.Roles.CountAsync());
        Assert.Equal(0, await dbContext.UserRoles.CountAsync());
        Assert.Equal(0, await dbContext.IdentityProfiles.CountAsync());
        Assert.Equal(0, await dbContext.Persons.CountAsync());
    }

    [Fact]
    public async Task RunAsync_SuccessResult_NeverExposesPasswordOrHash()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.RunAsync(ValidRequest);

        var properties = typeof(BootstrapResult).GetProperties().Select(p => p.Name).ToArray();
        Assert.Equal(["Success", "Message", "UserId"], properties);
        Assert.DoesNotContain(ValidRequest.Password, result.Message);
    }

    [Fact]
    public async Task RunAsync_DoesNotCreateAnyProperty()
    {
        using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        await service.RunAsync(ValidRequest);

        Assert.Equal(0, await dbContext.Properties.CountAsync());
    }

    private static MasterdomDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MasterdomDbContext(options);
    }

    private static BootstrapProvisioningService CreateService(MasterdomDbContext dbContext)
    {
        return new BootstrapProvisioningService(
            dbContext,
            new PersonRepository(dbContext),
            new RoleRepository(dbContext),
            new CredentialRepository(dbContext),
            new PasswordHasher());
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
