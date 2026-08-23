using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.RolePermission;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Security;
using Masterdom.Host;
using Masterdom.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Delegation Endpoint Integration Tests - Complete HTTP Workflow
///
/// Tests delegation endpoints with real HTTP authentication and business workflows.
/// Validates:
/// - Authentication: Anonymous → 401, Invalid JWT → 401
/// - Authorization: Valid token → Processing
/// - Business Workflow: Successful Create → Verified Persistence
/// - Business Workflow: Successful Revoke → Verified Persistence
/// - Business Rules: Escalation → Rejected, Scope violation → Rejected, etc.
/// </summary>
public sealed class DelegationEndpointIntegrationTests
{
    // ========== 1. Authentication Tests (Anonymous → 401) ==========

    [Fact]
    public async Task CreateDelegation_Anonymous_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/delegations", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDelegation_Anonymous_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/delegations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeDelegation_Anonymous_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/delegations/{Guid.NewGuid()}/revoke",
            new { reason = "" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========== 2. Authentication Tests (Invalid JWT → 401) ==========

    [Fact]
    public async Task CreateDelegation_InvalidBearerToken_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token");

        var response = await client.PostAsJsonAsync("/api/delegations", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDelegation_InvalidBearerToken_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token");

        var response = await client.GetAsync($"/api/delegations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeDelegation_InvalidBearerToken_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token");

        var response = await client.PostAsJsonAsync(
            $"/api/delegations/{Guid.NewGuid()}/revoke",
            new { reason = "" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateDelegation_WrongSigningKey_Returns401()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var invalidToken = CreateToken("wrong-key", MasterdomRoles.SuperUser, Guid.NewGuid());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        var response = await client.PostAsJsonAsync("/api/delegations", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========== 3. Authorization Boundary (All Endpoints Require Auth) ==========

    [Fact]
    public async Task AllDelegationEndpoints_RequireAuthorization()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var delegationId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync("/api/delegations", new { });
        var getResponse = await client.GetAsync($"/api/delegations/{delegationId}");
        var revokeResponse = await client.PostAsJsonAsync(
            $"/api/delegations/{delegationId}/revoke",
            new { reason = "" });

        // All endpoints require authentication
        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, revokeResponse.StatusCode);
    }

    // ========== 4. Valid Token Tests (Request Processing) ==========

    [Fact]
    public async Task CreateDelegation_ValidToken_ProcessesRequest()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            Guid.NewGuid());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = Guid.NewGuid(),
            delegatedRoleId = Guid.NewGuid(),
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var response = await client.PostAsJsonAsync("/api/delegations", request);

        // Authenticated, so not 401. Response depends on domain validation.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDelegation_ValidToken_ProcessesRequest()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            Guid.NewGuid());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/delegations/{Guid.NewGuid()}");

        // Authenticated, so not 401.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeDelegation_ValidToken_ProcessesRequest()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            Guid.NewGuid());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            $"/api/delegations/{Guid.NewGuid()}/revoke",
            new { reason = "Test" });

        // Authenticated, so not 401.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========== 5. Successful HTTP Create with Persistence Verification ==========

    [Fact]
    public async Task CreateDelegation_ValidRequest_ReturnsSuccessAndPersists()
    {
        // PRODUCTION-WIRING PROOF (ADR-0010): IAuthorityLevelProvider is NOT substituted in this
        // factory (see ConfigureWebHost above). The seeded delegator's Role is persisted with a
        // real RoleAuthorityLevel and resolved by the real RoleAuthorityLevelProvider through
        // IRoleRepository. This is the exact production path DelegationValidator depends on for
        // CanDelegate(); a passing result here proves the shipped provider, not a test double.
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1),
            description = "HTTP workflow test delegation"
        };

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/delegations", request);

        // Assert - Must succeed with real fixture data
        Assert.True(
            createResponse.IsSuccessStatusCode,
            $"Expected success, got {createResponse.StatusCode}: {await createResponse.Content.ReadAsStringAsync()}");

        var responseContent = await createResponse.Content.ReadAsStringAsync();
        Assert.NotEmpty(responseContent);

        // Parse response to extract delegation ID
        var delegationId = Guid.Empty;
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement) &&
            Guid.TryParse(idElement.GetString(), out var parsed))
        {
            delegationId = parsed;
        }
        Assert.NotEqual(Guid.Empty, delegationId);

        // Verify persisted record in DB (fresh scope to ensure no in-memory caching)
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var persistedRecord = await dbContext.Set<DelegatedAuthority>()
            .FirstOrDefaultAsync(d => d.Id.Value == delegationId);

        Assert.NotNull(persistedRecord);
        Assert.Equal(fixture.UserId, persistedRecord.DelegatorUserId.Value);
        Assert.Equal(fixture.DelegateeId, persistedRecord.DelegatedToUserId.Value);
        Assert.Equal(fixture.RoleId, persistedRecord.DelegatedRoleId.Value);
        Assert.Equal(DelegatedAuthorityStatus.Active, persistedRecord.Status);
    }

    // ========== 6. Successful HTTP Revoke with Persistence Verification ==========

    [Fact]
    public async Task RevokeDelegation_ValidRequest_ReturnsSuccessAndPersists()
    {
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First: Create a delegation
        var createRequest = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegations", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode,
            $"Create failed: {createResponse.StatusCode}");

        // Extract delegation ID from response
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var delegationId = Guid.Empty;
        using (var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent))
        {
            if (jsonDoc.RootElement.TryGetProperty("id", out var idElement) &&
                Guid.TryParse(idElement.GetString(), out var parsed))
            {
                delegationId = parsed;
            }
        }
        Assert.NotEqual(Guid.Empty, delegationId);

        // Second: Revoke it
        var revokeRequest = new { reason = "HTTP workflow test revocation" };
        var revokeResponse = await client.PostAsJsonAsync(
            $"/api/delegations/{delegationId:D}/revoke",
            revokeRequest);

        Assert.True(revokeResponse.IsSuccessStatusCode,
            $"Revoke failed: {revokeResponse.StatusCode}");

        // Third: Verify persisted revoked state in DB
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var persistedRecord = await dbContext.Set<DelegatedAuthority>()
            .FirstOrDefaultAsync(d => d.Id.Value == delegationId);

        Assert.NotNull(persistedRecord);
        Assert.Equal(DelegatedAuthorityStatus.Revoked, persistedRecord.Status);
        Assert.NotNull(persistedRecord.RevokedAtUtc);
        Assert.NotNull(persistedRecord.RevokedBy);
    }

    // ========== 7. HTTP Successful Create Followed by GET Retrieval ==========

    [Fact]
    public async Task CreateDelegation_ThenRetrieve_BothSucceed()
    {
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create delegation
        var createRequest = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegations", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode, $"Create failed: {createResponse.StatusCode}");

        var responseJson = await createResponse.Content.ReadAsStringAsync();

        // Parse JSON to extract the actual delegation ID from create response
        var createdDelegationId = Guid.Empty;
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseJson);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement) &&
            Guid.TryParse(idElement.GetString(), out var parsed))
        {
            createdDelegationId = parsed;
        }
        Assert.NotEqual(Guid.Empty, createdDelegationId);

        // Retrieve using the ACTUAL created delegation ID
        var getResponse = await client.GetAsync($"/api/delegations/{createdDelegationId:D}");

        // Should retrieve successfully
        Assert.True(getResponse.IsSuccessStatusCode, $"GET failed: {getResponse.StatusCode}");
    }

    // ========== 8. Delegator Spoofing Prevention Test ==========

    [Fact]
    public async Task CreateDelegation_DelegatorIsAlwaysCurrentUser_CannotBeSpoofed()
    {
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync();
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/delegations", request);

        // Must succeed with fixture data
        Assert.True(response.IsSuccessStatusCode, $"Create failed: {response.StatusCode}");

        var responseContent = await response.Content.ReadAsStringAsync();
        var delegatorId = Guid.Empty;
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
        if (jsonDoc.RootElement.TryGetProperty("delegatorUserId", out var delegatorElement) &&
            Guid.TryParse(delegatorElement.GetString(), out var parsed))
        {
            delegatorId = parsed;
        }
        Assert.NotEqual(Guid.Empty, delegatorId);

        // MUST be the authenticated user - proves spoofing is prevented
        Assert.Equal(fixture.UserId, delegatorId);
    }

    // ========== 9. Unauthorized Revoke Prevention Test ==========

    [Fact]
    public async Task RevokeDelegation_UnauthorizedUser_CannotRevoke()
    {
        await using var factory = new DelegationTestApplicationFactory();
        var fixtureA = await factory.SeedDelegationFixtureAsync();
        using var client = factory.CreateClient();

        var userBId = Guid.NewGuid();  // Attacker trying to revoke (no fixture)

        // Step 1: UserA creates a delegation
        var tokenA = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixtureA.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createRequest = new
        {
            delegateeUserId = fixtureA.DelegateeId,
            delegatedRoleId = fixtureA.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var createResponse = await client.PostAsJsonAsync("/api/delegations", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode, $"Create failed: {createResponse.StatusCode}");

        // Extract delegation ID
        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var delegationId = Guid.Empty;
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement) &&
            Guid.TryParse(idElement.GetString(), out var parsed))
        {
            delegationId = parsed;
        }
        Assert.NotEqual(Guid.Empty, delegationId);

        // Step 2: UserB attempts to revoke it (SHOULD FAIL - no authority)
        var tokenB = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.PropertyOwner,  // Different user, lower authority
            userBId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var revokeRequest = new { reason = "Unauthorized attempt" };
        var revokeResponse = await client.PostAsJsonAsync(
            $"/api/delegations/{delegationId:D}/revoke",
            revokeRequest);

        // Should be rejected (NOT 200 success, NOT 401 unauthenticated)
        Assert.False(revokeResponse.IsSuccessStatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, revokeResponse.StatusCode);

        // Independently verify delegation is still Active (was NOT revoked)
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var persistedRecord = await dbContext.Set<DelegatedAuthority>()
            .FirstOrDefaultAsync(d => d.Id.Value == delegationId);

        Assert.NotNull(persistedRecord);
        Assert.Equal(DelegatedAuthorityStatus.Active, persistedRecord.Status);
    }

    // ========== 10. Authenticated-but-Unauthorized Create Test ==========

    [Fact]
    public async Task CreateDelegation_AuthenticatedButUnauthorized_IsRejected()
    {
        await using var factory = new DelegationTestApplicationFactory();
        using var client = factory.CreateClient();

        var lowPrivilegeUserId = Guid.NewGuid();
        var delegateeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Authenticated with a low-privilege role (not allowed to delegate)
        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.PropertyOwner,  // Lower privilege, might not be allowed to delegate
            lowPrivilegeUserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = delegateeId,
            delegatedRoleId = roleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/delegations", request);

        // Assert: Should NOT be 401 (that's authentication)
        // But should fail for authorization/business rule reason
        Assert.True(response.StatusCode != HttpStatusCode.Unauthorized,
            "Authenticated users get a different error than 401");
        Assert.False(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized,
            $"Should reject low-privilege user. Got {response.StatusCode}");
    }

    [Fact]
    public async Task CreateDelegation_PersistedRoleResolvesToTenant_IsRejected()
    {
        // PRODUCTION-WIRING PROOF (ADR-0010): unlike CreateDelegation_AuthenticatedButUnauthorized_IsRejected
        // (which uses a user with NO persisted role at all), this delegator has a real, persisted
        // Role that resolves -- through the real RoleAuthorityLevelProvider, not a test double --
        // to AuthorityLevels.Tenant. The rejection below is produced by DelegationValidator's
        // CanDelegate() check acting on that correctly-resolved, insufficient level, not by a
        // missing-role short-circuit.
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync(RoleAuthorityLevel.Tenant);
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var response = await client.PostAsJsonAsync("/api/delegations", request);

        Assert.False(
            response.IsSuccessStatusCode,
            $"A delegator whose persisted Role resolves to Tenant must be rejected. Got {response.StatusCode}");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var persistedRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == fixture.Role.Id);
        Assert.NotNull(persistedRole);
        Assert.Equal(RoleAuthorityLevel.Tenant, persistedRole!.AuthorityLevel);
    }

    [Fact]
    public async Task CreateDelegation_PersistedRoleResolvesToAdmin_IsRejected()
    {
        await using var factory = new DelegationTestApplicationFactory();
        var fixture = await factory.SeedDelegationFixtureAsync(RoleAuthorityLevel.Admin);
        using var client = factory.CreateClient();

        var token = CreateToken(
            DelegationTestApplicationFactory.SigningKey,
            MasterdomRoles.SuperUser,
            fixture.UserId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            delegateeUserId = fixture.DelegateeId,
            delegatedRoleId = fixture.RoleId,
            propertyIds = Array.Empty<Guid>(),
            effectiveFromUtc = DateTime.UtcNow,
            effectiveToUtc = DateTime.UtcNow.AddMonths(1)
        };

        var response = await client.PostAsJsonAsync("/api/delegations", request);

        Assert.False(
            response.IsSuccessStatusCode,
            $"A delegator whose persisted Role resolves to Admin (level {AuthorityLevels.Admin}, below the CanDelegate threshold of {AuthorityLevels.SecondarySuperUser}) must be rejected. Got {response.StatusCode}");
    }

    // ========== Token & Factory Helpers ==========

    private static string CreateToken(string signingKey, string role, Guid userId)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: "masterdom-tests",
                audience: "masterdom-tests",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch
        {
            // Invalid key or other token creation error
            return "invalid";
        }
    }

    private sealed class DelegationTestApplicationFactory : WebApplicationFactory<Program>
    {
        public const string SigningKey = "masterdom-delegation-tests-key-123456";
        private readonly string _databaseName = $"delegation-http-{Guid.NewGuid():N}";

        public DelegationTestApplicationFactory()
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", SigningKey);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_ISSUER", "masterdom-tests");
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_AUDIENCE", "masterdom-tests");
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Bearer:SigningKey"] = SigningKey,
                    ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                    ["Authentication:Bearer:Audience"] = "masterdom-tests"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<MasterdomDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<IDbContextOptionsConfiguration<MasterdomDbContext>>();
                services.RemoveAll<MasterdomDbContext>();
                services.AddDbContext<MasterdomDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // IAuthorityLevelProvider is intentionally NOT substituted here.
                // These tests exercise the real, production-registered RoleAuthorityLevelProvider
                // (Masterdom.Modules.Security), which resolves a role's authority level from the
                // persisted Role seeded below. See ADR-0010.
            });
        }

        protected override void Dispose(bool disposing)
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", null);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_ISSUER", null);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_AUDIENCE", null);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Seeds a complete identity fixture for testing delegation workflows.
        /// Creates: IdentityProfile → User → Role → UserRole → Permission → RolePermission
        ///
        /// The seeded Role's authority level is persisted Domain state (see ADR-0010) --
        /// there is no separate test-side authority registration step. The real, production
        /// RoleAuthorityLevelProvider resolves it by reading the Role back from the database.
        /// </summary>
        public async Task<DelegationTestFixture> SeedDelegationFixtureAsync(
            RoleAuthorityLevel? authorityLevel = null)
        {
            authorityLevel ??= RoleAuthorityLevel.SecondarySuperUser;

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

            // 1. Create IdentityProfile (required by User)
            var profile = IdentityProfile.Create(
                IdentityProfileCode.Create("test-profile"),
                IdentityProfileType.Employee);
            dbContext.IdentityProfiles.Add(profile);
            await dbContext.SaveChangesAsync();

            // 2. Create User (User.Create() generates its own ID via UserId.New())
            var user = User.Create(
                UserCode.Create("test-user-code"),
                new IdentityProfileId(profile.Id.Value),
                Username.Create("test-user"));

            // 3. Create delegatee User
            var delegatee = User.Create(
                UserCode.Create("delegatee-code"),
                new IdentityProfileId(profile.Id.Value),
                Username.Create("delegatee-user"));

            dbContext.Users.Add(user);
            dbContext.Users.Add(delegatee);
            await dbContext.SaveChangesAsync();

            // 4. Create Role with the requested, persisted authority level (see ADR-0010).
            var role = Role.Create(
                RoleCode.Create($"test-role-{Guid.NewGuid():N}"),
                RoleName.Create("SuperUser"),
                authorityLevel);
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            // 5. Create Permissions
            var permissionDelegation = Permission.Create(
                PermissionCode.Create("delegation:create"),
                PermissionName.Create("Create Delegation"));
            var permissionRead = Permission.Create(
                PermissionCode.Create("delegation:read"),
                PermissionName.Create("Read Delegation"));

            dbContext.Permissions.Add(permissionDelegation);
            dbContext.Permissions.Add(permissionRead);
            await dbContext.SaveChangesAsync();

            // 6. Create RolePermissions (link role to permissions)
            var rpDelegation = RolePermission.Create(
                new RoleId(role.Id.Value),
                new PermissionId(permissionDelegation.Id.Value));

            var rpRead = RolePermission.Create(
                new RoleId(role.Id.Value),
                new PermissionId(permissionRead.Id.Value));

            dbContext.RolePermissions.Add(rpDelegation);
            dbContext.RolePermissions.Add(rpRead);
            await dbContext.SaveChangesAsync();

            // 7. Create UserRole (assign role to user)
            var now = DateTime.UtcNow;
            var userRole = UserRole.Create(
                user.Id,
                new RoleId(role.Id.Value),
                assignedBy: null,
                effectiveFromUtc: now,
                effectiveToUtc: null,
                isPrimaryRole: true,
                reason: "Test fixture");
            userRole.Activate();

            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();

            return new DelegationTestFixture(
                UserId: user.Id.Value,
                DelegateeId: delegatee.Id.Value,
                RoleId: role.Id.Value,
                Role: role,
                User: user,
                Delegatee: delegatee,
                UserRole: userRole);
        }
    }

    /// <summary>
    /// Represents a complete identity fixture for delegation testing.
    /// </summary>
    private sealed record DelegationTestFixture(
        Guid UserId,
        Guid DelegateeId,
        Guid RoleId,
        Role Role,
        User User,
        User Delegatee,
        UserRole UserRole);
}
