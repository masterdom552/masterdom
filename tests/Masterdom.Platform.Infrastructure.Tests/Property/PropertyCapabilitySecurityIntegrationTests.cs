using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Masterdom.Core.Security;
using Masterdom.Host;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Masterdom.Platform.Infrastructure.Tests.Property;

public sealed class PropertyCapabilitySecurityIntegrationTests
{
    [Fact]
    public async Task Anonymous_Request_ShouldBeRejected()
    {
        await using var factory = new PropertyCapabilityApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/properties/search");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InvalidBearerToken_ShouldBeRejected()
    {
        await using var factory = new PropertyCapabilityApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(
                signingKey: "not-the-configured-signing-key-1234567890",
                role: MasterdomRoles.SuperUser,
                userId: Guid.NewGuid()));

        var response = await client.GetAsync("/api/properties/search");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SuperUser_Request_ShouldSucceed()
    {
        await using var factory = new PropertyCapabilityApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader(
            CreateToken(PropertyCapabilityApplicationFactory.SigningKey, MasterdomRoles.SuperUser, Guid.NewGuid()));

        var response = await client.GetAsync("/api/properties/search");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PropertyOwner_ShouldAccessOwnedProperty_AndBeForbiddenForUnownedProperty()
    {
        var ownerUserId = Guid.NewGuid();

        await using var factory = new PropertyCapabilityApplicationFactory();
        var seeded = await factory.SeedPropertyScenarioAsync(ownerUserId);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader(
            CreateToken(PropertyCapabilityApplicationFactory.SigningKey, MasterdomRoles.PropertyOwner, ownerUserId));

        var ownedResponse = await client.GetAsync($"/api/properties/{seeded.OwnedPropertyId}");
        var unownedResponse = await client.GetAsync($"/api/properties/{seeded.UnownedPropertyId}");

        Assert.Equal(HttpStatusCode.OK, ownedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, unownedResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_ShouldReadOwnPerson_ButNotAnotherPerson()
    {
        var tenantUserId = Guid.NewGuid();

        await using var factory = new PropertyCapabilityApplicationFactory();
        var seeded = await factory.SeedPeopleScenarioAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader(
            CreateToken(
                PropertyCapabilityApplicationFactory.SigningKey,
                MasterdomRoles.Tenant,
                tenantUserId,
                personId: seeded.OwnPersonId));

        var ownReadResponse = await client.GetAsync($"/api/people/{seeded.OwnPersonId}");
        var otherReadResponse = await client.GetAsync($"/api/people/{seeded.OtherPersonId}");

        Assert.Equal(HttpStatusCode.OK, ownReadResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, otherReadResponse.StatusCode);
    }

    [Fact]
    public void Tenant_Handler_ShouldUpdateOwnPerson()
    {
        var databaseName = $"tenant-self-update-{Guid.NewGuid():N}";

        Guid personId;
        using (var seedProvider = BuildProvider(CurrentUser.Anonymous, databaseName))
        using (var seedScope = seedProvider.CreateScope())
        {
            var dbContext = seedScope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
            var person = Person.Create(
                PersonNumber.Create("TEN-HANDLER-01"),
                PersonName.Create("Taylor", "Tenant"),
                Gender.Create("Other"));

            dbContext.Persons.Add(person);
            dbContext.SaveChanges();
            personId = person.Id.Value;
        }

        var currentUser = CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: personId,
            username: "tenant-self-handler",
            roles: [MasterdomRoles.Tenant],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>());

        using var provider = BuildProvider(currentUser, databaseName);
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<Masterdom.Modules.People.Application.Support.ICommandHandler<Masterdom.Modules.People.Application.Commands.RenamePersonCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>>>();

        var result = handler.Handle(
            new Masterdom.Modules.People.Application.Commands.RenamePersonCommand(
                Masterdom.Core.Identifiers.PersonId.From(personId),
                PersonName.Create("Taylor", "Updated")));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Taylor Updated", result.Value!.Name.DisplayName);
    }

    [Fact]
    public void Anonymous_Handler_ShouldBeRejected()
    {
        using var provider = BuildProvider(CurrentUser.Anonymous);
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<Masterdom.Modules.Properties.Application.Support.ICommandHandler<CreatePropertyCommand, Masterdom.Modules.Properties.Application.Support.ExecutionResult<Masterdom.Modules.Properties.Domain.Entities.Property.Property>>>();

        var result = handler.Handle(new CreatePropertyCommand(new PropertyCode("AUTH-001"), new PropertyName("Auth Property"), PropertyType.Commercial));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
    }

    [Fact]
    public void PropertyRepository_ShouldFilterOwnedProperties()
    {
        var ownerUserId = Guid.NewGuid();
        var currentUser = CurrentUser.Authenticated(
            userId: ownerUserId,
            personId: null,
            username: "owner-filter",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>());

        using var provider = BuildProvider(currentUser);
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();
        var ownedProperty = Masterdom.Modules.Properties.Domain.Entities.Property.Property.Create(
            new PropertyCode("OWN-001"),
            new PropertyName("Owned Property"),
            PropertyType.Commercial);
        ownedProperty.ChangeOwner(ownerUserId);

        var otherProperty = Masterdom.Modules.Properties.Domain.Entities.Property.Property.Create(
            new PropertyCode("OWN-002"),
            new PropertyName("Other Property"),
            PropertyType.Commercial);
        otherProperty.ChangeOwner(Guid.NewGuid());

        dbContext.Properties.AddRange(ownedProperty, otherProperty);
        dbContext.SaveChanges();

        var repository = scope.ServiceProvider.GetRequiredService<Masterdom.Modules.Properties.Domain.Repositories.IPropertyRepository>();
        var results = repository.Search(null, 10);

        Assert.Single(results);
        Assert.Equal(ownedProperty.Id, results.Single().Id);
    }

    private static AuthenticationHeaderValue CreateAuthorizationHeader(string token)
    {
        return new AuthenticationHeaderValue("Bearer", token);
    }

    private static string CreateToken(string signingKey, string role, Guid userId, Guid? personId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "pkg-4c-test-user"),
            new(ClaimTypes.Role, role)
        };

        if (role == MasterdomRoles.Manager)
        {
            claims.Add(new Claim(MasterdomClaimTypes.Permission, "properties.read"));
            claims.Add(new Claim(MasterdomClaimTypes.Permission, "properties.manage"));
        }

        if (personId.HasValue)
        {
            claims.Add(new Claim(MasterdomClaimTypes.PersonId, personId.Value.ToString()));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "masterdom-tests",
            audience: "masterdom-tests",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static ServiceProvider BuildProvider(CurrentUser currentUser, string? databaseName = null)
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName ?? $"property-capability-security-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(currentUser));
        services.AddScoped<Masterdom.Modules.Properties.Application.Support.IPropertyUnitOfWork, PassThroughPropertyUnitOfWork>();
        services.AddScoped<Masterdom.Modules.People.Application.Support.IPersonUnitOfWork, PassThroughPersonUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Lease.Application.Support.ILeaseUnitOfWork, PassThroughLeaseUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Tenancy.Application.Support.ITenancyUnitOfWork, PassThroughTenancyUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class PropertyCapabilityApplicationFactory : WebApplicationFactory<Program>
    {
        public const string SigningKey = "masterdom-tests-signing-key-123456";
        private readonly string _databaseName = $"property-capability-http-{Guid.NewGuid():N}";

        public PropertyCapabilityApplicationFactory()
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", SigningKey);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_ISSUER", "masterdom-tests");
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_AUDIENCE", "masterdom-tests");
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Masterdom"] = "Host=localhost;Database=masterdom_tests;Username=test;Password=test",
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
            });
        }

        protected override void Dispose(bool disposing)
        {
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY", null);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_ISSUER", null);
            Environment.SetEnvironmentVariable("MASTERDOM_AUTHENTICATION_AUDIENCE", null);
            base.Dispose(disposing);
        }

        public async Task<(Guid OwnedPropertyId, Guid UnownedPropertyId)> SeedPropertyScenarioAsync(Guid ownerUserId)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

            var ownedProperty = Masterdom.Modules.Properties.Domain.Entities.Property.Property.Create(
                new PropertyCode("HTTP-OWN-01"),
                new PropertyName("Owned HTTP Property"),
                PropertyType.Commercial);
            ownedProperty.ChangeOwner(ownerUserId);

            var unownedProperty = Masterdom.Modules.Properties.Domain.Entities.Property.Property.Create(
                new PropertyCode("HTTP-OWN-02"),
                new PropertyName("Unowned HTTP Property"),
                PropertyType.Commercial);
            unownedProperty.ChangeOwner(Guid.NewGuid());

            dbContext.Properties.AddRange(ownedProperty, unownedProperty);
            await dbContext.SaveChangesAsync();

            return (ownedProperty.Id.Value, unownedProperty.Id.Value);
        }

        public async Task<(Guid OwnPersonId, Guid OtherPersonId)> SeedPeopleScenarioAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

            var ownPerson = Person.Create(
                PersonNumber.Create("TEN-SELF-01"),
                PersonName.Create("Taylor", "Tenant"),
                Gender.Create("Other"));

            var otherPerson = Person.Create(
                PersonNumber.Create("TEN-OTHER-01"),
                PersonName.Create("Morgan", "Other"),
                Gender.Create("Other"));

            dbContext.Persons.Add(ownPerson);
            dbContext.Persons.Add(otherPerson);
            await dbContext.SaveChangesAsync();

            return (ownPerson.Id.Value, otherPerson.Id.Value);
        }
    }

    private sealed class FixedCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;

        public FixedCurrentUserAccessor(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public CurrentUser GetCurrentUser() => _currentUser;
    }

    private sealed class PassThroughPropertyUnitOfWork : Masterdom.Modules.Properties.Application.Support.IPropertyUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPropertyUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughPersonUnitOfWork : Masterdom.Modules.People.Application.Support.IPersonUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPersonUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughLeaseUnitOfWork : Masterdom.Modules.Lease.Application.Support.ILeaseUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughLeaseUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }

    private sealed class PassThroughTenancyUnitOfWork : Masterdom.Modules.Tenancy.Application.Support.ITenancyUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughTenancyUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Action operation)
        {
            operation();
            _dbContext.SaveChanges();
        }
    }
}
