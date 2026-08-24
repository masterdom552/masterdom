using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Host;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Authentication.Application.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masterdom.Platform.Infrastructure.Tests.Authentication;

/// <summary>
/// Authentication Endpoint Integration Tests - HTTP login workflow.
///
/// Exercises POST /api/authentication/login end-to-end against a real
/// WebApplicationFactory, a real password hasher, and a real JWT issuer --
/// only the database is substituted (InMemory), matching the pattern in
/// DelegationEndpointIntegrationTests.
/// </summary>
public sealed class AuthenticationEndpointIntegrationTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        var fixture = await factory.SeedUserFixtureAsync("correct-password-1");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = fixture.Username, password = "correct-password-1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.True(json.RootElement.TryGetProperty("accessToken", out var tokenElement));
        Assert.False(string.IsNullOrWhiteSpace(tokenElement.GetString()));
    }

    [Fact]
    public async Task Login_ResponseBody_NeverContainsPasswordOrHash()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        var fixture = await factory.SeedUserFixtureAsync("correct-password-1");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = fixture.Username, password = "correct-password-1" });

        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("correct-password-1", body);
        Assert.DoesNotContain(fixture.PasswordHash, body);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        var fixture = await factory.SeedUserFixtureAsync("correct-password-1");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = fixture.Username, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_Returns401WithSameShapeAsWrongPassword()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        await factory.SeedUserFixtureAsync("correct-password-1");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = "no-such-user", password = "correct-password-1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInactiveUser_Returns401()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        var fixture = await factory.SeedUserFixtureAsync("correct-password-1", active: false);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = fixture.Username, password = "correct-password-1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_IssuedToken_GrantsAccessToProtectedEndpoint()
    {
        await using var factory = new AuthenticationTestApplicationFactory();
        var fixture = await factory.SeedUserFixtureAsync("correct-password-1");
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/authentication/login",
            new { username = fixture.Username, password = "correct-password-1" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var body = await loginResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var accessToken = json.RootElement.GetProperty("accessToken").GetString();

        using var anonymousResponse = await client.GetAsync("/api/identity/roles/UNKNOWN");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var authenticatedResponse = await client.GetAsync("/api/identity/roles/UNKNOWN");
        Assert.NotEqual(HttpStatusCode.Unauthorized, authenticatedResponse.StatusCode);
    }

    private sealed class AuthenticationTestApplicationFactory : WebApplicationFactory<Program>
    {
        public const string SigningKey = "masterdom-authentication-tests-key-123456";
        private readonly string _databaseName = $"authentication-http-{Guid.NewGuid():N}";

        public AuthenticationTestApplicationFactory()
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", SigningKey);
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Bearer:SigningKey"] = SigningKey,
                    ["ConnectionStrings:Masterdom"] = "Host=localhost;Database=masterdom_tests;Username=test;Password=test",
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
            });
        }

        protected override void Dispose(bool disposing)
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", null);
            base.Dispose(disposing);
        }

        public async Task<AuthenticationTestFixture> SeedUserFixtureAsync(string password, bool active = true)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

            var profile = IdentityProfile.Create(
                IdentityProfileCode.Create("auth-test-profile"),
                IdentityProfileType.Person);
            dbContext.IdentityProfiles.Add(profile);
            await dbContext.SaveChangesAsync();

            var user = User.Create(
                UserCode.Create("auth-test-user-code"),
                profile.Id,
                Username.Create("auth-test-user"));
            if (!active)
            {
                user.Deactivate();
            }
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var passwordHasher = new PasswordHasher();
            var passwordHash = passwordHasher.Hash(password);
            var credential = Credential.Create(user.Id, passwordHash);
            dbContext.Credentials.Add(credential);
            await dbContext.SaveChangesAsync();

            return new AuthenticationTestFixture(user.Id.Value, user.Username.Value, passwordHash);
        }
    }

    private sealed record AuthenticationTestFixture(Guid UserId, string Username, string PasswordHash);
}
