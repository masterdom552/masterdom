using Masterdom.Core.Identifiers;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Lease;
using Masterdom.Infrastructure.Persistence.People;
using Masterdom.Infrastructure.Persistence.Tenancy;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using LeaseEffectiveDate = Masterdom.Modules.Lease.Domain.Entities.Lease.EffectiveDate;
using LeasePropertyReference = Masterdom.Modules.Lease.Domain.Entities.Lease.PropertyReference;
using LeaseUnitReference = Masterdom.Modules.Lease.Domain.Entities.Lease.UnitReference;
using TenancyNotes = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Notes;
using TenancyPropertyReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.PropertyReference;
using TenancyUnitReference = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.UnitReference;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Platform.Infrastructure.Tests.Property;

public sealed class PropertyCapabilityRepositoryTests
{
    [Fact]
    public void PersonRepository_ShouldResolveByNumber()
    {
        using var dbContext = CreateDbContext();
        var repository = new PersonRepository(dbContext);

        var person = Person.Create(PersonNumber.Create("PS-001"), PersonName.Create("Alex", "Tenant"), Gender.Other);
        repository.Add(person);
        dbContext.SaveChanges();

        var loaded = repository.GetByNumber(PersonNumber.Create("PS-001"));

        Assert.NotNull(loaded);
        Assert.Equal(person.Id, loaded!.Id);
    }

    [Fact]
    public void LeaseRepository_ShouldReportActiveLeaseForTenancy()
    {
        using var dbContext = CreateDbContext();
        var repository = new LeaseRepository(dbContext, new FixedCurrentUserAccessor(CurrentUser.Anonymous));

        var tenancyReference = TenancyReference.Create(Guid.NewGuid());

        var lease = LeaseAggregate.Create(
            LeaseNumber.Create("LS-REPO-01"),
            LeaseType.Residential,
            tenancyReference,
            LeasePropertyReference.Create(Guid.NewGuid()),
            LeaseUnitReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                LeaseEffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(),
            BuildClauses());

        lease.Activate();
        repository.Add(lease);
        dbContext.SaveChanges();

        Assert.True(repository.HasActiveLeaseForTenancy(tenancyReference));
    }

    [Fact]
    public void TenancyRepository_ShouldReportActiveTenancyForUnit()
    {
        using var dbContext = CreateDbContext();
        var repository = new TenancyRepository(dbContext, new FixedCurrentUserAccessor(CurrentUser.Anonymous));

        var unitReference = TenancyUnitReference.Create(Guid.NewGuid());
        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-REPO-01"),
            TenancyPropertyReference.Create(Guid.NewGuid()),
            unitReference,
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            TenancyNotes.Create("Repo test"));

        repository.Add(tenancy);
        dbContext.SaveChanges();

        Assert.True(repository.HasActiveTenancyForUnit(unitReference));
    }

    private static MasterdomDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase($"property-capability-repo-{Guid.NewGuid():N}")
            .Options;

        return new MasterdomDbContext(options);
    }

    private static CommercialTerms BuildCommercialTerms()
    {
        return CommercialTerms.Create(
            RentTerms.Create(1200m, BillingFrequency.Monthly, 5, 3),
            DepositTerms.Create(900m, true, SecurityDepositReference.Create("DEP-REPO"), "config.deposit.default"),
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

    private sealed class FixedCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;

        public FixedCurrentUserAccessor(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public CurrentUser GetCurrentUser() => _currentUser;
    }
}
