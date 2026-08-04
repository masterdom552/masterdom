using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.Billability;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.MonthlyBilling;
using Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling;

public sealed class MonthlyBillingApplicationServiceTests
{
    [Fact]
    public void Execute_ShouldDelegatePersistence_ToBillPersistenceCapability()
    {
        var candidate = CreateBillabilityCandidate();
        var persistenceCapability = new SpyBillPersistenceCapability();
        var service = new MonthlyBillingApplicationService(
            new BillabilityDeterminationService(),
            new ChargeCompositionPipeline([
                new FakeChargeSource(_ => [CreateChargeCandidate(candidate, 900m)])
            ]),
            persistenceCapability,
            new MonthlyBillFactory(),
            new DefaultBillNumberGenerator());

        var result = service.Execute(CreateRequest([CreateProjection(candidate)]));

        Assert.Equal(1, persistenceCapability.PersistCallCount);
        Assert.Single(persistenceCapability.LastRequest!.Bills);
        Assert.Single(result.GeneratedBills);
        Assert.Equal(1, result.Summary.BillsGenerated);
    }

    [Fact]
    public void Execute_ShouldRejectMixedCurrencies_ForSingleBillSnapshot()
    {
        var candidate = CreateBillabilityCandidate();
        var service = new MonthlyBillingApplicationService(
            new BillabilityDeterminationService(),
            new ChargeCompositionPipeline([
                new FakeChargeSource(_ =>
                [
                    CreateChargeCandidate(candidate, 900m, "USD"),
                    CreateChargeCandidate(candidate, 100m, "EUR")
                ])
            ]),
            new SpyBillPersistenceCapability(),
            new MonthlyBillFactory(),
            new DefaultBillNumberGenerator());

        var exception = Assert.Throws<InvalidOperationException>(() => service.Execute(CreateRequest([CreateProjection(candidate)])));

        Assert.Equal("Exactly one currency is required per bill snapshot.", exception.Message);
    }

    [Fact]
    public void Execute_ShouldPreserveSkippedBillCount_WithLocalClampBehavior()
    {
        var candidate = CreateBillabilityCandidate();
        var persistenceCapability = new SpyBillPersistenceCapability();
        var service = new MonthlyBillingApplicationService(
            new BillabilityDeterminationService(),
            new ChargeCompositionPipeline([
                new FakeChargeSource(_ => [CreateChargeCandidate(candidate, 900m)])
            ]),
            persistenceCapability,
            new MonthlyBillFactory(),
            new DefaultBillNumberGenerator());

        var result = service.Execute(CreateRequest([CreateProjection(candidate)]));

        Assert.Equal(0, result.Summary.CandidatesSkipped);
    }

    private static MonthlyBillingRequest CreateRequest(IReadOnlyCollection<BillabilityResolutionRequest.CandidateProjection> projections)
    {
        return new MonthlyBillingRequest(
            CreateBillingContext(),
            projections,
            generatedDate: new DateOnly(2026, 8, 1),
            issueDate: new DateOnly(2026, 8, 1),
            dueDate: new DateOnly(2026, 8, 10));
    }

    private static BillingContext CreateBillingContext()
    {
        return new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 1, 0), DateTimeKind.Utc));
    }

    private static BillabilityCandidate CreateBillabilityCandidate()
    {
        return new BillabilityCandidate(
            TenancyReference.Create(Guid.NewGuid()),
            LeaseReference.Create(Guid.NewGuid()),
            PropertyReference.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            PersonReference.Create(PersonId.New()));
    }

    private static BillabilityResolutionRequest.CandidateProjection CreateProjection(BillabilityCandidate candidate)
    {
        return new BillabilityResolutionRequest.CandidateProjection(
            candidate.TenancyReference,
            candidate.LeaseReference,
            candidate.PropertyReference,
            candidate.UnitId,
            candidate.PrimaryOccupantReference,
            LeaseStatus: "Active",
            LeaseEffectiveDate: new DateOnly(2026, 1, 1),
            LeaseExpiryDate: new DateOnly(2026, 12, 31),
            TenancyStatus: "Active",
            MoveInDate: new DateOnly(2025, 12, 1),
            MoveOutDate: null,
            UnitOccupancyStatus: "Occupied");
    }

    private static ChargeCandidate CreateChargeCandidate(BillabilityCandidate candidate, decimal amount, string currency = "USD")
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TenancyId"] = candidate.TenancyReference!.TenancyId.ToString("D"),
            ["LeaseId"] = candidate.LeaseReference!.LeaseId.ToString("D"),
            ["PropertyId"] = candidate.PropertyReference!.PropertyId.ToString("D")
        };

        return new ChargeCandidate(
            chargeType: "Rent",
            description: "Rent charge",
            amount: amount,
            currency: currency,
            sourceCapability: "Rent",
            externalReference: candidate.LeaseReference!.LeaseId.ToString("N"),
            metadata: metadata);
    }

    private sealed class FakeChargeSource : IChargeSource
    {
        private readonly Func<ChargeCompositionRequest, IReadOnlyCollection<ChargeCandidate>> _compose;

        public FakeChargeSource(Func<ChargeCompositionRequest, IReadOnlyCollection<ChargeCandidate>> compose)
        {
            _compose = compose;
        }

        public string ProviderId => "FakeMonthly";

        public IReadOnlyCollection<ChargeCandidate> Compose(ChargeCompositionRequest request)
        {
            return _compose(request);
        }
    }

    private sealed class SpyBillPersistenceCapability : BillPersistenceCapability
    {
        public SpyBillPersistenceCapability()
            : base(new NullBillPersistenceService())
        {
        }

        public int PersistCallCount { get; private set; }

        public BillPersistenceRequest? LastRequest { get; private set; }

        public override BillPersistenceResult Persist(BillPersistenceRequest request)
        {
            PersistCallCount++;
            LastRequest = request;
            return new BillPersistenceResult(request.Bills);
        }
    }

    private sealed class NullBillPersistenceService : IBillPersistenceService
    {
        public BillPersistenceResult Persist(BillPersistenceRequest request)
        {
            return new BillPersistenceResult(request.Bills);
        }
    }
}
