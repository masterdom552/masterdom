using Masterdom.Core.Identifiers;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Tenancy;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Platform.Infrastructure.Tests.Persistence.Tenancy;

/// <summary>
/// Proves TenancyRepository's corrected queries (PKG-CAP-023) translate
/// correctly against SQLite. The PropertyOwner branch uses a two-query pattern
/// (materialize owned property GUIDs from Properties table, convert to
/// PropertyReference objects, then Contains filter on Tenancies). The Manager
/// branch converts propertyScopes Guids to PropertyReference objects and uses
/// Contains. Both patterns would throw InvalidOperationException against the
/// original member-access-on-converted-property predicate shape.
/// </summary>
public sealed class TenancyRepositoryRelationalTests
{
    [Fact]
    public void GetById_PropertyOwner_OwnedProperty_ReturnsTenancy()
    {
        using var fixture = CreateSqliteContext();
        var ownerUserId = Guid.NewGuid();

        var (tenancy, _) = SeedTenancyWithProperty(fixture.DbContext, ownerUserId);

        var repository = new TenancyRepository(fixture.DbContext, AsPropertyOwner(ownerUserId));
        var result = repository.GetById(tenancy.Id);

        Assert.NotNull(result);
        Assert.Equal(tenancy.Id, result.Id);
    }

    [Fact]
    public void GetById_PropertyOwner_NonOwnedProperty_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();
        var differentOwnerUserId = Guid.NewGuid();

        var (tenancy, _) = SeedTenancyWithProperty(fixture.DbContext, differentOwnerUserId);

        var unrelatedOwner = Guid.NewGuid();
        var repository = new TenancyRepository(fixture.DbContext, AsPropertyOwner(unrelatedOwner));
        var result = repository.GetById(tenancy.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_Manager_ScopedProperty_ReturnsTenancy()
    {
        using var fixture = CreateSqliteContext();

        var (tenancy, property) = SeedTenancyWithProperty(fixture.DbContext, ownerId: null);

        var repository = new TenancyRepository(fixture.DbContext, AsManager(property.Id.Value));
        var result = repository.GetById(tenancy.Id);

        Assert.NotNull(result);
        Assert.Equal(tenancy.Id, result.Id);
    }

    [Fact]
    public void GetById_Manager_OutOfScopeProperty_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var (tenancy, _) = SeedTenancyWithProperty(fixture.DbContext, ownerId: null);

        var repository = new TenancyRepository(fixture.DbContext, AsManager(Guid.NewGuid()));
        var result = repository.GetById(tenancy.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_Unauthenticated_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var (tenancy, _) = SeedTenancyWithProperty(fixture.DbContext, ownerId: null);

        var repository = new TenancyRepository(fixture.DbContext, AsUnauthenticated());
        var result = repository.GetById(tenancy.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_SuperUser_ReturnsTenancy()
    {
        using var fixture = CreateSqliteContext();

        var (tenancy, _) = SeedTenancyWithProperty(fixture.DbContext, ownerId: null);

        var repository = new TenancyRepository(fixture.DbContext, AsSuperUser());
        var result = repository.GetById(tenancy.Id);

        Assert.NotNull(result);
        Assert.Equal(tenancy.Id, result.Id);
    }

    private static (TenancyAggregate Tenancy, PropertyAggregate Property) SeedTenancyWithProperty(
        MasterdomDbContext dbContext, Guid? ownerId)
    {
        var property = PropertyAggregate.Create(
            new PropertyCode("PROP-001"),
            new PropertyName("Test Property"),
            PropertyType.Residential);
        if (ownerId.HasValue)
            property.ChangeOwner(ownerId.Value);
        dbContext.Properties.Add(property);
        dbContext.SaveChanges();

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("T-001"),
            PropertyReference.Create(property.Id.Value),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.Today)),
            OccupantReference.Create(PersonId.New(), isPrimary: true),
            notes: null);
        dbContext.Tenancies.Add(tenancy);
        dbContext.SaveChanges();

        return (tenancy, property);
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

    private static ICurrentUserAccessor AsPropertyOwner(Guid userId) =>
        new FixedCurrentUserAccessor(CurrentUser.Authenticated(
            userId: userId,
            personId: null,
            username: "owner",
            roles: [MasterdomRoles.PropertyOwner],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: false));

    private static ICurrentUserAccessor AsManager(params Guid[] propertyScopes) =>
        new FixedCurrentUserAccessor(CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "manager",
            roles: [MasterdomRoles.Manager],
            permissions: [],
            propertyScopes: propertyScopes,
            ownedPropertyIds: [],
            isInherentSuperUser: false));

    private static ICurrentUserAccessor AsUnauthenticated() =>
        new FixedCurrentUserAccessor(CurrentUser.Anonymous);

    private static ICurrentUserAccessor AsSuperUser() =>
        new FixedCurrentUserAccessor(CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: [],
            propertyScopes: [],
            ownedPropertyIds: [],
            isInherentSuperUser: true));

    private sealed class FixedCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;
        public FixedCurrentUserAccessor(CurrentUser currentUser) => _currentUser = currentUser;
        public CurrentUser GetCurrentUser() => _currentUser;
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

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
        }
    }
}
