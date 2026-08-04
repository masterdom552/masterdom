using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Rent;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Pipeline;

public sealed class ChargeCompositionPipelineTests
{
    [Fact]
    public void Compose_ShouldExecuteSingleProvider_AndReturnCharge()
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
            RentAmount: 1200m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-1"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.Equal(5, result.ChargeCandidates.Count);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "Rent" && x.Amount == 1200m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "UtilityReference" && x.Amount == 25m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "Maintenance" && x.Amount == 50m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "CarryForward" && x.Amount == 10m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "OneTime" && x.Amount == 5m);

        Assert.Equal(5, trace.ExecutedProviders.Count);
        Assert.Equal(RentChargeSource.ProviderIdentifier, trace.ExecutedProviders.ElementAt(0).ProviderId);
        Assert.Equal(0, trace.ExecutedProviders.ElementAt(0).ExecutionOrder);
    }

    [Fact]
    public void Compose_ShouldExecuteIChargeSourceImplementations_Sequentially()
    {
        var fixture = BuildFixture();
        var executionOrder = new List<string>();

        var firstSource = new FakeOrderedChargeSource("first", executionOrder,
        [
            new ChargeCandidate("Rent", "First", 10m, "USD", "SourceA")
        ]);

        var secondSource = new FakeOrderedChargeSource("second", executionOrder,
        [
            new ChargeCandidate("Rent", "Second", 20m, "USD", "SourceB")
        ]);

        var pipeline = new ChargeCompositionPipeline(
            new IChargeSource[] { firstSource, secondSource });

        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.Equal(new[] { "first", "second" }, executionOrder);
        Assert.Equal(2, result.ChargeCandidates.Count);
        Assert.Equal(10m, result.ChargeCandidates.ElementAt(0).Amount);
        Assert.Equal(20m, result.ChargeCandidates.ElementAt(1).Amount);
        Assert.Equal(2, trace.ExecutedProviders.Count);
        Assert.Equal("first", trace.ExecutedProviders.ElementAt(0).ProviderId);
        Assert.Equal(0, trace.ExecutedProviders.ElementAt(0).ExecutionOrder);
        Assert.Equal("second", trace.ExecutedProviders.ElementAt(1).ProviderId);
        Assert.Equal(1, trace.ExecutedProviders.ElementAt(1).ExecutionOrder);
    }

    [Fact]
    public void Compose_ShouldReturnEmptyCharges_WhenProviderReturnsEmpty()
    {
        var fixture = BuildFixture();
        var pipeline = new ChargeCompositionPipeline(new FakeChargeCompositionReadService());

        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.Equal(4, result.ChargeCandidates.Count);
        Assert.DoesNotContain(result.ChargeCandidates, x => x.ChargeType == "Rent");
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "UtilityReference" && x.Amount == 25m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "Maintenance" && x.Amount == 50m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "CarryForward" && x.Amount == 10m);
        Assert.Contains(result.ChargeCandidates, x => x.ChargeType == "OneTime" && x.Amount == 5m);
        Assert.Equal(5, trace.ExecutedProviders.Count);
    }

    [Fact]
    public void Compose_ShouldAggregateMultipleCharges_FromRentSource()
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
            LeaseNumber: "LS-PIPE-2"));
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
            LeaseNumber: "LS-PIPE-3"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(request, out var trace);

        Assert.Equal(10, result.ChargeCandidates.Count);
        Assert.Contains(result.ChargeCandidates, x => x.Amount == 800m);
        Assert.Contains(result.ChargeCandidates, x => x.Amount == 950m);
        Assert.Equal(5, trace.ExecutedProviders.Count);
    }

    [Fact]
    public void Compose_ShouldMaintainDeterministicOrdering()
    {
        var first = BuildFixture();
        var second = BuildFixture();
        var request = BuildRequest(first.BillingContext, first.Candidate, second.Candidate);

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            first.TenancyId,
            first.LeaseId,
            first.PropertyId,
            first.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 101m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-4"));
        readService.Add(new RentChargeReadModel(
            second.TenancyId,
            second.LeaseId,
            second.PropertyId,
            second.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 202m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-5"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(request, out var trace);

        Assert.Equal("Rent", result.ChargeCandidates.ElementAt(0).ChargeType);
        Assert.Equal(101m, result.ChargeCandidates.ElementAt(0).Amount);
        Assert.Equal("Rent", result.ChargeCandidates.ElementAt(1).ChargeType);
        Assert.Equal(202m, result.ChargeCandidates.ElementAt(1).Amount);
        Assert.Equal("UtilityReference", result.ChargeCandidates.ElementAt(2).ChargeType);
        Assert.Equal("UtilityReference", result.ChargeCandidates.ElementAt(3).ChargeType);
        Assert.Equal("Maintenance", result.ChargeCandidates.ElementAt(4).ChargeType);
        Assert.Equal("Maintenance", result.ChargeCandidates.ElementAt(5).ChargeType);
        Assert.Equal("CarryForward", result.ChargeCandidates.ElementAt(6).ChargeType);
        Assert.Equal("CarryForward", result.ChargeCandidates.ElementAt(7).ChargeType);
        Assert.Equal("OneTime", result.ChargeCandidates.ElementAt(8).ChargeType);
        Assert.Equal("OneTime", result.ChargeCandidates.ElementAt(9).ChargeType);
        Assert.Equal(10, result.ChargeCandidates.Count);
        Assert.Equal(RentChargeSource.ProviderIdentifier, trace.ExecutedProviders.ElementAt(0).ProviderId);
        Assert.Equal(0, trace.ExecutedProviders.ElementAt(0).ExecutionOrder);
    }

    [Fact]
    public void RentChargeSource_ShouldImplementIChargeSource()
    {
        Assert.Contains(typeof(IChargeSource), typeof(RentChargeSource).GetInterfaces());
        Assert.Equal(RentChargeSource.ProviderIdentifier, new RentChargeSource(new FakeChargeCompositionReadService()).ProviderId);
    }

    [Fact]
    public void Compose_ShouldAggregateResultShape()
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
            RentAmount: 777m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-6"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.NotEmpty(result.ChargeCandidates);
        Assert.NotEmpty(trace.ExecutedProviders);
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("ExecutedProviders"));
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("Warnings"));
    }

    [Fact]
    public void Compose_ResultShouldExposeOnlyChargeCandidates_ForCanonicalCollection()
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
            RentAmount: 500m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-ONLY"));

        var pipeline = new ChargeCompositionPipeline(readService);
        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.NotNull(result.ChargeCandidates);
        Assert.NotNull(trace.ExecutedProviders);
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("Charges"));
    }

    [Fact]
    public void Compose_ShouldReturnImmutableCollections()
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
            RentAmount: 111m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-PIPE-7"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(fixture.Request, out var trace);

        Assert.False(result.ChargeCandidates is List<ChargeCandidate>);
        Assert.False(trace.ExecutedProviders is List<ExecutedProvider>);
    }

    [Fact]
    public void Compose_ShouldNotDuplicateCandidates_WhenProviderReturnsUniqueItems()
    {
        var first = BuildFixture();
        var second = BuildFixture();
        var request = BuildRequest(first.BillingContext, first.Candidate, second.Candidate);

        var readService = new FakeChargeCompositionReadService();
        readService.Add(new RentChargeReadModel(
            first.TenancyId,
            first.LeaseId,
            first.PropertyId,
            first.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 300m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-UNIQ-1"));
        readService.Add(new RentChargeReadModel(
            second.TenancyId,
            second.LeaseId,
            second.PropertyId,
            second.UnitId,
            IsTenancyActive: true,
            IsLeaseActive: true,
            RentAmount: 301m,
            Currency: "USD",
            BillingFrequency: "Monthly",
            LeaseNumber: "LS-UNIQ-2"));

        var pipeline = new ChargeCompositionPipeline(readService);

        var result = pipeline.Compose(request, out var trace);

        var externalReferences = result.ChargeCandidates.Select(x => x.ExternalReference).ToList();
        Assert.Equal(8, externalReferences.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(10, result.ChargeCandidates.Count);
        Assert.Equal(5, trace.ExecutedProviders.Count);
    }

    private static PipelineFixture BuildFixture()
    {
        var tenancyId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        var candidate = new BillabilityCandidate(
            TenancyReference.Create(tenancyId),
            LeaseReference.Create(leaseId),
            PropertyReference.Create(propertyId),
            unitId,
            PersonReference.Create(PersonId.New()));

        var context = CreateBillingContext();
        var request = BuildRequest(context, candidate);

        return new PipelineFixture(tenancyId, leaseId, propertyId, unitId, context, candidate, request);
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
            correlationId: "corr-pipeline-1");
    }

    private sealed record PipelineFixture(
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

    private sealed class FakeOrderedChargeSource : IChargeSource
    {
        public string ProviderId { get; }

        private readonly string _name;
        private readonly IList<string> _executionOrder;
        private readonly IReadOnlyCollection<ChargeCandidate> _candidates;

        public FakeOrderedChargeSource(
            string name,
            IList<string> executionOrder,
            IReadOnlyCollection<ChargeCandidate> candidates)
        {
            ProviderId = name;
            _name = name;
            _executionOrder = executionOrder;
            _candidates = candidates;
        }

        public IReadOnlyCollection<ChargeCandidate> Compose(ChargeCompositionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _executionOrder.Add(_name);
            return _candidates;
        }
    }
}
