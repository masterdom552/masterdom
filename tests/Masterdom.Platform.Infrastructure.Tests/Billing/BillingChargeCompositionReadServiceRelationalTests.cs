using Masterdom.Core.Identifiers;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Billing;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using LeaseEffectiveDate = Masterdom.Modules.Lease.Domain.Entities.Lease.EffectiveDate;
using LeasePropertyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.PropertyReference;
using LeaseTenancyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.TenancyReference;
using LeaseUnitReference = Masterdom.Modules.Lease.Domain.Entities.Lease.UnitReference;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;
using TenancyNotes = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Notes;
using TenancyPropertyReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.PropertyReference;
using TenancyUnitReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.UnitReference;

namespace Masterdom.Platform.Infrastructure.Tests.Billing;

/// <summary>
/// Proves BillingChargeCompositionReadService.GetRentChargeReadModel's
/// corrected queries (CAP-023 Phase 4) against a real relational EF Core
/// provider (SQLite) -- both the Lease lookup (LeaseId.From(leaseId)) and
/// the Tenancy lookup (TenancyId.From(tenancyId)) were previously .Value
/// member accesses that could not translate against Npgsql.
/// </summary>
public sealed class BillingChargeCompositionReadServiceRelationalTests
{
    [Fact]
    public async Task GetRentChargeReadModel_WithMatchingIdentifiers_ReturnsReadModel()
    {
        using var fixture = CreateSqliteContext();

        var propertyId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create($"TEN-{Guid.NewGuid():N}"[..20]),
            TenancyPropertyReference.Create(propertyId),
            TenancyUnitReference.Create(unitId),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            TenancyNotes.Create("Relational test tenancy"));
        fixture.DbContext.Tenancies.Add(tenancy);
        await fixture.DbContext.SaveChangesAsync();

        var lease = LeaseAggregate.Create(
            LeaseNumber.Create($"LS-{Guid.NewGuid():N}"[..20]),
            LeaseType.Residential,
            LeaseTenancyReference.Create(tenancy.Id.Value),
            LeasePropertyReference.Create(propertyId),
            LeaseUnitReference.Create(unitId),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                LeaseEffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(),
            BuildClauses());
        lease.Activate();
        fixture.DbContext.Leases.Add(lease);
        await fixture.DbContext.SaveChangesAsync();

        var service = new BillingChargeCompositionReadService(fixture.DbContext);

        var readModel = service.GetRentChargeReadModel(
            tenancyId: tenancy.Id.Value,
            leaseId: lease.Id.Value,
            propertyId: propertyId,
            unitId: unitId);

        Assert.NotNull(readModel);
        Assert.Equal(tenancy.Id.Value, readModel!.TenancyId);
        Assert.Equal(lease.Id.Value, readModel.LeaseId);
        Assert.Equal(propertyId, readModel.PropertyId);
        Assert.Equal(unitId, readModel.UnitId);
        Assert.True(readModel.IsLeaseActive);
        Assert.True(readModel.IsTenancyActive);
        Assert.Equal(1200m, readModel.RentAmount);
    }

    [Fact]
    public async Task GetRentChargeReadModel_WithNonMatchingLeaseId_ReturnsNull()
    {
        using var fixture = CreateSqliteContext();

        var propertyId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create($"TEN-{Guid.NewGuid():N}"[..20]),
            TenancyPropertyReference.Create(propertyId),
            TenancyUnitReference.Create(unitId),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            TenancyNotes.Create("Relational test tenancy"));
        fixture.DbContext.Tenancies.Add(tenancy);
        await fixture.DbContext.SaveChangesAsync();

        var lease = LeaseAggregate.Create(
            LeaseNumber.Create($"LS-{Guid.NewGuid():N}"[..20]),
            LeaseType.Residential,
            LeaseTenancyReference.Create(tenancy.Id.Value),
            LeasePropertyReference.Create(propertyId),
            LeaseUnitReference.Create(unitId),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                LeaseEffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(),
            BuildClauses());
        lease.Activate();
        fixture.DbContext.Leases.Add(lease);
        await fixture.DbContext.SaveChangesAsync();

        var service = new BillingChargeCompositionReadService(fixture.DbContext);

        var readModel = service.GetRentChargeReadModel(
            tenancyId: tenancy.Id.Value,
            leaseId: Guid.NewGuid(),
            propertyId: propertyId,
            unitId: unitId);

        Assert.Null(readModel);
    }

    private static CommercialTerms BuildCommercialTerms()
    {
        return CommercialTerms.Create(
            RentTerms.Create(1200m, BillingFrequency.Monthly, 5, 3),
            DepositTerms.Create(900m, true, SecurityDepositReference.Create("DEP-REL"), "config.deposit.default"),
            RenewalTerms.Create(false, 30, "config.renewal.standard"),
            TerminationTerms.Create(30, "config.termination.standard", "config.latefee.standard"));
    }

    private static LeaseClauses BuildClauses()
    {
        return LeaseClauses.Create(
            ClauseCollection.Create([
                LeaseClause.Create("BASE", "Base lease clause")
            ]));
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

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
        }
    }
}
