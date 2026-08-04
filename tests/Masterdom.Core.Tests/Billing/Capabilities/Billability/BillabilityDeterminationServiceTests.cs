using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Capabilities.Billability;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.Capabilities.Billability;

public sealed class BillabilityDeterminationServiceTests
{
    [Fact]
    public void Determine_ShouldIncludeCandidate_WhenAllEligibilityConditionsPass()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection());

        var result = service.Determine(request);

        Assert.Single(result.IncludedCandidates);
        Assert.Empty(result.ExcludedCandidates);
        Assert.Equal(1, result.TotalEvaluated);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenLeaseIsFutureForBillingPeriod()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(
            leaseStatus: "Draft",
            leaseEffectiveDate: new DateOnly(2026, 9, 1),
            leaseExpiryDate: new DateOnly(2027, 8, 31)));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Contains(BillabilityExclusionReason.FutureLease, excluded.Decision.Reasons);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenLeaseIsExpiredForBillingPeriod()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(
            leaseStatus: "Expired",
            leaseEffectiveDate: new DateOnly(2025, 1, 1),
            leaseExpiryDate: new DateOnly(2025, 12, 31)));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Contains(BillabilityExclusionReason.ExpiredLease, excluded.Decision.Reasons);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenTenancyIsInactive()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(tenancyStatus: "Closed"));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Contains(BillabilityExclusionReason.InactiveTenancy, excluded.Decision.Reasons);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenUnitIsVacant()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(unitOccupancyStatus: "Vacant"));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Contains(BillabilityExclusionReason.VacantUnit, excluded.Decision.Reasons);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenReferencesAreMissing()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(
            tenancyReference: null,
            leaseReference: null,
            propertyReference: null,
            unitId: Guid.Empty));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Equal([BillabilityExclusionReason.MissingReference], excluded.Decision.Reasons);
    }

    [Fact]
    public void Determine_ShouldExcludeCandidate_WhenOutsideBillingPeriod()
    {
        var service = new BillabilityDeterminationService();
        var request = CreateRequest(CreateProjection(
            moveInDate: new DateOnly(2026, 10, 1),
            moveOutDate: null,
            leaseEffectiveDate: new DateOnly(2026, 10, 1),
            leaseExpiryDate: new DateOnly(2027, 9, 30)));

        var result = service.Determine(request);

        var excluded = Assert.Single(result.ExcludedCandidates);
        Assert.Contains(BillabilityExclusionReason.OutsideBillingPeriod, excluded.Decision.Reasons);
    }

    private static BillabilityResolutionRequest CreateRequest(BillabilityResolutionRequest.CandidateProjection projection)
    {
        return new BillabilityResolutionRequest(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            [projection]);
    }

    private static BillabilityResolutionRequest.CandidateProjection CreateProjection(
        TenancyReference? tenancyReference = null,
        LeaseReference? leaseReference = null,
        PropertyReference? propertyReference = null,
        Guid? unitId = null,
        PersonReference? primaryOccupantReference = null,
        string leaseStatus = "Active",
        DateOnly? leaseEffectiveDate = null,
        DateOnly? leaseExpiryDate = null,
        string tenancyStatus = "Active",
        DateOnly? moveInDate = null,
        DateOnly? moveOutDate = null,
        string unitOccupancyStatus = "Occupied")
    {
        return new BillabilityResolutionRequest.CandidateProjection(
            tenancyReference ?? TenancyReference.Create(Guid.NewGuid()),
            leaseReference ?? LeaseReference.Create(Guid.NewGuid()),
            propertyReference ?? PropertyReference.Create(Guid.NewGuid()),
            unitId ?? Guid.NewGuid(),
            primaryOccupantReference ?? PersonReference.Create(PersonId.New()),
            leaseStatus,
            leaseEffectiveDate ?? new DateOnly(2026, 1, 1),
            leaseExpiryDate ?? new DateOnly(2026, 12, 31),
            tenancyStatus,
            moveInDate ?? new DateOnly(2025, 12, 1),
            moveOutDate,
            unitOccupancyStatus);
    }
}
