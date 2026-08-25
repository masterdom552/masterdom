using Masterdom.Core.Identifiers;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Lease;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Platform.Infrastructure.Tests.Persistence.Lease;

/// <summary>
/// Proves LeaseRepository's corrected queries (PKG-CAP-023) translate
/// correctly against SQLite. The PropertyOwner branch uses a two-query pattern
/// (materialize owned property GUIDs from Properties table, convert to
/// PropertyReference objects, then Contains filter on Leases). The Manager
/// branch converts propertyScopes Guids to PropertyReference objects and uses
/// Contains. Both patterns would throw InvalidOperationException against the
/// original member-access-on-converted-property predicate shape.
/// </summary>
public sealed class LeaseRepositoryRelationalTests
{
    [Fact]
    public void GetById_PropertyOwner_OwnedProperty_ReturnsLease()
    {
        using var fixture = CreateSqliteContext();
        var ownerUserId = Guid.NewGuid();

        var (lease, _) = SeedLeaseWithProperty(fixture.DbContext, ownerUserId);

        var repository = new LeaseRepository(fixture.DbContext, AsPropertyOwner(ownerUserId));
        var result = repository.GetById(lease.Id);

        Assert.NotNull(result);
        Assert.Equal(lease.Id, result.Id);
    }

    [Fact]
    public void GetById_PropertyOwner_NonOwnedProperty_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();
        var differentOwnerUserId = Guid.NewGuid();

        var (lease, _) = SeedLeaseWithProperty(fixture.DbContext, differentOwnerUserId);

        var unrelatedOwner = Guid.NewGuid();
        var repository = new LeaseRepository(fixture.DbContext, AsPropertyOwner(unrelatedOwner));
        var result = repository.GetById(lease.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_Manager_ScopedProperty_ReturnsLease()
    {
        using var fixture = CreateSqliteContext();

        var (lease, property) = SeedLeaseWithProperty(fixture.DbContext, ownerId: null);

        var repository = new LeaseRepository(fixture.DbContext, AsManager(property.Id.Value));
        var result = repository.GetById(lease.Id);

        Assert.NotNull(result);
        Assert.Equal(lease.Id, result.Id);
    }

    [Fact]
    public void GetById_Manager_OutOfScopeProperty_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var (lease, _) = SeedLeaseWithProperty(fixture.DbContext, ownerId: null);

        var repository = new LeaseRepository(fixture.DbContext, AsManager(Guid.NewGuid()));
        var result = repository.GetById(lease.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetByNumber_Manager_ScopedProperty_ReturnsLease()
    {
        using var fixture = CreateSqliteContext();

        var (lease, property) = SeedLeaseWithProperty(fixture.DbContext, ownerId: null);

        var repository = new LeaseRepository(fixture.DbContext, AsManager(property.Id.Value));
        var result = repository.GetByNumber(lease.Number);

        Assert.NotNull(result);
        Assert.Equal(lease.Id, result.Id);
    }

    [Fact]
    public void GetById_Unauthenticated_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var (lease, _) = SeedLeaseWithProperty(fixture.DbContext, ownerId: null);

        var repository = new LeaseRepository(fixture.DbContext, AsUnauthenticated());
        var result = repository.GetById(lease.Id);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_SuperUser_ReturnsLease()
    {
        using var fixture = CreateSqliteContext();

        var (lease, _) = SeedLeaseWithProperty(fixture.DbContext, ownerId: null);

        var repository = new LeaseRepository(fixture.DbContext, AsSuperUser());
        var result = repository.GetById(lease.Id);

        Assert.NotNull(result);
        Assert.Equal(lease.Id, result.Id);
    }

    private static (LeaseAggregate Lease, PropertyAggregate Property) SeedLeaseWithProperty(
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

        var lease = BuildLease("L-001", property.Id.Value);
        dbContext.Leases.Add(lease);
        dbContext.SaveChanges();

        return (lease, property);
    }

    private static LeaseAggregate BuildLease(string number, Guid propertyGuid)
    {
        return LeaseAggregate.Create(
            LeaseNumber.Create(number),
            LeaseType.Residential,
            TenancyReference.Create(Guid.NewGuid()),
            Masterdom.Modules.Lease.Domain.Entities.Lease.PropertyReference.Create(propertyGuid),
            Masterdom.Modules.Lease.Domain.Entities.Lease.UnitReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.Today)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.Today.AddYears(1)))),
            CommercialTerms.Create(
                RentTerms.Create(1000m, BillingFrequency.Monthly, rentDueDay: 1, gracePeriodDays: 5),
                DepositTerms.Create(2000m, isRefundable: true,
                    SecurityDepositReference.Create("DEP-REF-001"), "deposit-policy"),
                RenewalTerms.Create(autoRenew: false, noticePeriodDays: 30, "renewal-policy"),
                TerminationTerms.Create(noticePeriodDays: 30, "termination-policy", "late-fee-policy")),
            LeaseClauses.Create(
                ClauseCollection.Create([LeaseClause.Create("NO-PETS", "No pets allowed.")])));
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
