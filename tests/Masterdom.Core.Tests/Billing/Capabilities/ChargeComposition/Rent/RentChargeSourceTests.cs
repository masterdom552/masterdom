using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Rent;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Rent;

public sealed class RentChargeSourceTests
{
    [Fact]
    public void Determine_ShouldReturnRentCandidate_ForValidBillableTenancy()
    {
        var fixture = BuildFixture();

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            fixture.TenancyId,
            fixture.LeaseId,
            fixture.PropertyId,
            fixture.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 1500m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-1001"));

        var source = new RentChargeSource(readService);

        var result = source.Compose(fixture.Request);

        var charge = Assert.Single(result);
        Assert.Equal("Rent", charge.ChargeType);
        Assert.Equal("Rent charge", charge.Description);
        Assert.Equal(1500m, charge.Amount);
        Assert.Equal("USD", charge.Currency);
        Assert.Equal("Rent", charge.SourceCapability);
        Assert.Equal("LS-1001", charge.ExternalReference);
        Assert.Equal(fixture.LeaseId.ToString("D"), charge.Metadata["LeaseId"]);
        Assert.Equal(fixture.TenancyId.ToString("D"), charge.Metadata["TenancyId"]);
        Assert.Equal(fixture.PropertyId.ToString("D"), charge.Metadata["PropertyId"]);
    }

    [Fact]
    public void Determine_ShouldReturnNoCandidates_WhenTenancyIsNotBillable()
    {
        var fixture = BuildFixture();

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            fixture.TenancyId,
            fixture.LeaseId,
            fixture.PropertyId,
            fixture.UnitId,
            IsTenancyActive: false,
            IsLeaseActive: true,
            RentAmount: 900m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-1002"));

        var source = new RentChargeSource(readService);

        var result = source.Compose(fixture.Request);

        Assert.Empty(result);
    }

    [Fact]
    public void Determine_ShouldReturnNoCandidates_WhenLeaseIsMissing()
    {
        var fixture = BuildFixture();

        var source = new RentChargeSource(new FakeChargeCompositionReadService());

        var result = source.Compose(fixture.Request);

        Assert.Empty(result);
    }

    [Fact]
    public void Determine_ShouldReturnNoCandidates_WhenRentIsMissing()
    {
        var fixture = BuildFixture();

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            fixture.TenancyId,
            fixture.LeaseId,
            fixture.PropertyId,
            fixture.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: null,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-1003"));

        var source = new RentChargeSource(readService);

        var result = source.Compose(fixture.Request);

        Assert.Empty(result);
    }

    [Fact]
    public void Determine_ShouldReturnMultipleCandidates_WhenMultipleValidCandidatesExist()
    {
        var left = BuildFixture();
        var right = BuildFixture();

        var request = BuildRequest(left.BillingContext, left.Candidate, right.Candidate);

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            left.TenancyId,
            left.LeaseId,
            left.PropertyId,
            left.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 800m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-2001"));
        readService.Add(new RentChargeReadModel(
            right.TenancyId,
            right.LeaseId,
            right.PropertyId,
            right.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 950m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-2002"));

        var source = new RentChargeSource(readService);

        var result = source.Compose(request);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Amount == 800m);
        Assert.Contains(result, x => x.Amount == 950m);
    }

    [Fact]
    public void Determine_ShouldReturnEmptyResult_WhenNoIncludedCandidates()
    {
        var billingContext = CreateBillingContext();
        var request = new ChargeCompositionRequest(
            billingContext,
            new BillabilityResolutionResult(
                Array.Empty<BillabilityCandidate>(),
                [
                    new ExcludedBillabilityCandidate(
                        new BillabilityCandidate(null, null, null, null, null),
                        BillabilityDecision.Excluded([BillabilityExclusionReason.MissingReference]))
                ]));

        var source = new RentChargeSource(
            new FakeChargeCompositionReadService());

        var result = source.Compose(request);

        Assert.Empty(result);
    }

    [Fact]
    public void Determine_ShouldReturnImmutableCandidateCollection()
    {
        var fixture = BuildFixture();

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            fixture.TenancyId,
            fixture.LeaseId,
            fixture.PropertyId,
            fixture.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 1100m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-1004"));

        var source = new RentChargeSource(readService);

        var result = source.Compose(fixture.Request);

        Assert.False(result is List<ChargeCandidate>);

        var mutablePropertyExists = typeof(ChargeCandidate)
            .GetProperties()
            .Any(x => x.SetMethod is not null && x.SetMethod.IsPublic);

        Assert.False(mutablePropertyExists);
    }

    private static RentFixture BuildFixture()
    {
        var unitId = Guid.NewGuid();
        var tenancyId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var candidate = new BillabilityCandidate(
            Masterdom.Modules.Billing.Domain.Entities.Billing.TenancyReference.Create(tenancyId),
            Masterdom.Modules.Billing.Domain.Entities.Billing.LeaseReference.Create(leaseId),
            Masterdom.Modules.Billing.Domain.Entities.Billing.PropertyReference.Create(propertyId),
            unitId,
            Masterdom.Modules.Billing.Domain.Entities.Billing.PersonReference.Create(PersonId.New()));

        var billingContext = CreateBillingContext();
        var request = BuildRequest(billingContext, candidate);

        return new RentFixture(tenancyId, leaseId, propertyId, unitId, billingContext, candidate, request);
    }

    private static ChargeCompositionRequest BuildRequest(BillingContext context, params BillabilityCandidate[] candidates)
    {
        return new ChargeCompositionRequest(
            context,
            new BillabilityResolutionResult(candidates, Array.Empty<ExcludedBillabilityCandidate>()));
    }

    private static BillingContext CreateBillingContext()
    {
        return new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc),
            propertyReference: null,
            unitReference: null,
            correlationId: "corr-rent-1");
    }

    private sealed record RentFixture(
        Guid TenancyId,
        Guid LeaseId,
        Guid PropertyId,
        Guid UnitId,
        BillingContext BillingContext,
        BillabilityCandidate Candidate,
        ChargeCompositionRequest Request);

    private sealed class FakeChargeCompositionReadService : IChargeCompositionReadService
    {
        private readonly Dictionary<(Guid TenancyId, Guid LeaseId, Guid PropertyId, Guid UnitId), RentChargeReadModel> _models = new();

        public void Add(RentChargeReadModel model)
        {
            _models[(model.TenancyId, model.LeaseId, model.PropertyId, model.UnitId)] = model;
        }

        public RentChargeReadModel? GetRentChargeReadModel(
            Guid tenancyId,
            Guid leaseId,
            Guid propertyId,
            Guid unitId)
        {
            return _models.TryGetValue((tenancyId, leaseId, propertyId, unitId), out var model)
                ? model
                : null;
        }
    }
}
