using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Property;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Platform.Infrastructure.Tests.Persistence.Property;

/// <summary>
/// Proves PropertyRepository's corrected Manager-branch query (PKG-CAP-023)
/// translates correctly against SQLite. The fix converts propertyScopes Guids
/// to PropertyId objects and uses Contains against x.Id, which would throw
/// InvalidOperationException against the original .Value-inside-IQueryable
/// predicate shape if restored.
/// </summary>
public sealed class PropertyRepositoryRelationalTests
{
    [Fact]
    public void GetById_Manager_ScopedProperty_ReturnsProperty()
    {
        using var fixture = CreateSqliteContext();

        var property = SeedProperty(fixture.DbContext);

        var repository = new PropertyRepository(fixture.DbContext, AsManager(property.Id.Value));
        var result = repository.GetById(property.Id);

        Assert.NotNull(result);
        Assert.Equal(property.Id, result.Id);
    }

    [Fact]
    public void GetById_Manager_OutOfScopeProperty_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var property = SeedProperty(fixture.DbContext);

        var repository = new PropertyRepository(fixture.DbContext, AsManager(Guid.NewGuid()));
        var result = repository.GetById(property.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_Manager_EmptyScopes_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var property = SeedProperty(fixture.DbContext);

        var repository = new PropertyRepository(fixture.DbContext, AsManager());
        var result = repository.GetById(property.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetByCode_Manager_ScopedProperty_ReturnsProperty()
    {
        using var fixture = CreateSqliteContext();

        var property = SeedProperty(fixture.DbContext);

        var repository = new PropertyRepository(fixture.DbContext, AsManager(property.Id.Value));
        var result = repository.GetByCode(property.Code);

        Assert.NotNull(result);
        Assert.Equal(property.Id, result.Id);
    }

    [Fact]
    public void GetById_SuperUser_ReturnsProperty()
    {
        using var fixture = CreateSqliteContext();

        var property = SeedProperty(fixture.DbContext);

        var repository = new PropertyRepository(fixture.DbContext, AsSuperUser());
        var result = repository.GetById(property.Id);

        Assert.NotNull(result);
        Assert.Equal(property.Id, result.Id);
    }

    private static PropertyAggregate SeedProperty(MasterdomDbContext dbContext)
    {
        var property = PropertyAggregate.Create(
            new PropertyCode("PROP-001"),
            new PropertyName("Test Property"),
            PropertyType.Residential);
        dbContext.Properties.Add(property);
        dbContext.SaveChanges();
        return property;
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
