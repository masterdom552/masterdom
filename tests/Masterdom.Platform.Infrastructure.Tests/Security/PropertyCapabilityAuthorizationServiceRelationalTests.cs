using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Security;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves PropertyCapabilityAuthorizationService.OwnsResolvedProperty's
/// corrected query (CAP-023 Phase 4) against a real relational EF Core
/// provider (SQLite), exercised through the real, production-registered
/// IPropertyCapabilityAuthorizationService (resolved via DI, since the
/// concrete class is internal to Masterdom.Infrastructure -- mirroring
/// LoginAuthorityResolverTests' own established pattern of testing through
/// the real DI-registered implementation, not a hand-written fake), so the
/// actual EF predicate is what gets translated and executed.
/// </summary>
public sealed class PropertyCapabilityAuthorizationServiceRelationalTests
{
    private static readonly CapabilityAuthorizationPolicy PropertyOwnerScopedPolicy = new(
        Operation: "test.property-owner-scoped",
        RequiredPermission: null,
        IsPropertyScoped: true,
        AllowsPropertyOwner: true,
        AllowsTenantSelf: false);

    [Fact]
    public async Task Authorize_WithPropertyOwnerOwningTheResolvedProperty_Allows()
    {
        using var fixture = CreateSqliteContext();

        var ownerId = Guid.NewGuid();
        var property = PropertyAggregate.Create(
            new PropertyCode($"REL-{Guid.NewGuid():N}"),
            new PropertyName("Relational Test Property"),
            PropertyType.Residential);
        property.ChangeOwner(ownerId);
        fixture.DbContext.Properties.Add(property);
        await fixture.DbContext.SaveChangesAsync();

        var currentUser = CurrentUser.Authenticated(
            ownerId, personId: null, "owner-user",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: []);

        var service = fixture.BuildAuthorizationService(currentUser, PropertyOwnerScopedPolicy);

        var result = service.Authorize(new AuthorizationContext(
            PropertyOwnerScopedPolicy.Operation, PropertyId: property.Id.Value));

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task Authorize_WithPropertyOwnerNotOwningTheResolvedProperty_Forbids()
    {
        using var fixture = CreateSqliteContext();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var property = PropertyAggregate.Create(
            new PropertyCode($"REL-{Guid.NewGuid():N}"),
            new PropertyName("Relational Test Property"),
            PropertyType.Residential);
        property.ChangeOwner(ownerId);
        fixture.DbContext.Properties.Add(property);
        await fixture.DbContext.SaveChangesAsync();

        var currentUser = CurrentUser.Authenticated(
            otherUserId, personId: null, "other-user",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: []);

        var service = fixture.BuildAuthorizationService(currentUser, PropertyOwnerScopedPolicy);

        var result = service.Authorize(new AuthorizationContext(
            PropertyOwnerScopedPolicy.Operation, PropertyId: property.Id.Value));

        Assert.False(result.IsAllowed);
    }

    private static SqliteDbContextFixture CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new MasterdomDbContext(options);
        dbContext.Database.EnsureCreated();

        return new SqliteDbContextFixture(dbContext, connection);
    }

    private sealed class SqliteDbContextFixture : IDisposable
    {
        public SqliteDbContextFixture(MasterdomDbContext dbContext, SqliteConnection connection)
        {
            DbContext = dbContext;
            _connection = connection;
        }

        public MasterdomDbContext DbContext { get; }

        private readonly SqliteConnection _connection;

        public IPropertyCapabilityAuthorizationService BuildAuthorizationService(
            CurrentUser currentUser,
            CapabilityAuthorizationPolicy policy)
        {
            var services = new ServiceCollection();
            services.AddSingleton(DbContext);
            services.AddSecurityInfrastructureRuntime();
            services.AddSingleton<ICurrentUserAccessor>(new FixedCurrentUserAccessor(currentUser));
            services.AddSingleton<ICapabilityAuthorizationPolicyProvider>(new FixedPolicyProvider(policy));

            var provider = services.BuildServiceProvider(validateScopes: true);
            var scope = provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IPropertyCapabilityAuthorizationService>();
        }

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
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

    private sealed class FixedPolicyProvider : ICapabilityAuthorizationPolicyProvider
    {
        private readonly CapabilityAuthorizationPolicy _policy;

        public FixedPolicyProvider(CapabilityAuthorizationPolicy policy)
        {
            _policy = policy;
        }

        public CapabilityAuthorizationPolicy GetPolicy(string operation) => _policy;
    }
}
