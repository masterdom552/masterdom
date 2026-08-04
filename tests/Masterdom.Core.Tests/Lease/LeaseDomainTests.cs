using Masterdom.Core.Identifiers;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Lease.Domain.Entities.Lease.Events;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Core.Tests.Lease;

public sealed class LeaseDomainTests
{
    [Fact]
    public void Create_ShouldInitializeDraftLeaseAndRaiseCreatedEvent()
    {
        var lease = LeaseAggregate.Create(
            LeaseNumber.Create("LS-0001"),
            LeaseType.Residential,
            TenancyReference.Create(Guid.NewGuid()),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(),
            BuildLeaseClauses());

        Assert.Equal(LeaseStatus.Draft, lease.Status);
        Assert.Single(lease.Versions);
        Assert.Contains(lease.DomainEvents, x => x is LeaseCreatedDomainEvent);
    }

    [Fact]
    public void Activate_ShouldMarkCurrentVersionActive_AndRaiseEvent()
    {
        var lease = CreateLease();

        lease.Activate();

        Assert.Equal(LeaseStatus.Active, lease.Status);
        Assert.Single(lease.Versions.Where(x => x.IsActive));
        Assert.Contains(lease.DomainEvents, x => x is LeaseActivatedDomainEvent);
    }

    [Fact]
    public void Renew_ShouldCreateNewActiveVersion_AndRaiseEvent()
    {
        var lease = CreateLease();
        lease.Activate();

        lease.Renew(
            RenewalDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11))),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(24)))),
            BuildCommercialTerms(monthlyRent: 1500m),
            BuildLeaseClauses("RENEWAL"));

        Assert.Equal(2, lease.Versions.Count);
        Assert.Single(lease.Versions.Where(x => x.IsActive));
        Assert.Equal(2, lease.CurrentVersion.VersionNumber);
        Assert.Contains(lease.DomainEvents, x => x is LeaseRenewedDomainEvent);
    }

    [Fact]
    public void Renew_ShouldThrow_WhenLeaseIsNotActive()
    {
        var lease = CreateLease();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            lease.Renew(
                RenewalDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11))),
                EffectivePeriod.Create(
                    EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))),
                    ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(24)))),
                BuildCommercialTerms(monthlyRent: 1500m),
                BuildLeaseClauses("RENEWAL")));

        Assert.Equal("Only active leases can be renewed.", exception.Message);
    }

    [Fact]
    public void ChangeCommercialTerms_ShouldVersionTerms_AndRaiseEvent()
    {
        var lease = CreateLease();
        lease.Activate();

        lease.ChangeCommercialTerms(
            BuildCommercialTerms(monthlyRent: 2000m),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))));

        Assert.Equal(2, lease.Versions.Count);
        Assert.Equal(2000m, lease.CurrentVersion.CommercialTerms.RentTerms.MonthlyRent);
        Assert.Contains(lease.DomainEvents, x => x is CommercialTermsChangedDomainEvent);
    }

    [Fact]
    public void Close_ShouldMakeLeaseImmutable()
    {
        var lease = CreateLease();
        lease.Close();

        var exception = Assert.Throws<InvalidOperationException>(() => lease.Expire());

        Assert.Equal("Closed lease cannot be modified.", exception.Message);
    }

    private static LeaseAggregate CreateLease()
    {
        return LeaseAggregate.Create(
            LeaseNumber.Create("LS-BASE"),
            LeaseType.Residential,
            TenancyReference.Create(Guid.NewGuid()),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            PersonReference.Create(PersonId.New()),
            EffectivePeriod.Create(
                EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                ExpiryDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))),
            BuildCommercialTerms(),
            BuildLeaseClauses());
    }

    private static CommercialTerms BuildCommercialTerms(decimal monthlyRent = 1200m)
    {
        return CommercialTerms.Create(
            RentTerms.Create(monthlyRent, BillingFrequency.Monthly, rentDueDay: 5, gracePeriodDays: 3),
            DepositTerms.Create(
                depositAmount: 1000m,
                isRefundable: true,
                SecurityDepositReference.Create("DEP-001"),
                depositRulesReference: "config.deposit.default"),
            RenewalTerms.Create(autoRenew: false, noticePeriodDays: 30, renewalPolicyReference: "config.renewal.standard"),
            TerminationTerms.Create(noticePeriodDays: 30, terminationPolicyReference: "config.termination.standard", lateFeePolicyReference: "config.latefee.standard"));
    }

    private static LeaseClauses BuildLeaseClauses(string code = "BASE")
    {
        return LeaseClauses.Create(
            ClauseCollection.Create([
                LeaseClause.Create(code, "Standard lease clause text")
            ]));
    }
}
