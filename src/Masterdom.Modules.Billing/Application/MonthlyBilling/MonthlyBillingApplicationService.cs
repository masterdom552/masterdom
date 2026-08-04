using Masterdom.Modules.Billing.Application.Capabilities.Billability;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling;

public sealed class MonthlyBillingApplicationService
{
    private readonly BillabilityDeterminationService _billabilityService;
    private readonly ChargeCompositionPipeline _chargeCompositionPipeline;
    private readonly BillPersistenceCapability _billPersistenceCapability;
    private readonly MonthlyBillFactory _billFactory;
    private readonly IBillNumberGenerator _billNumberGenerator;

    public MonthlyBillingApplicationService(
        BillabilityDeterminationService billabilityService,
        ChargeCompositionPipeline chargeCompositionPipeline,
        BillPersistenceCapability billPersistenceCapability)
        : this(
            billabilityService,
            chargeCompositionPipeline,
            billPersistenceCapability,
            new MonthlyBillFactory(),
            new DefaultBillNumberGenerator())
    {
    }

    public MonthlyBillingApplicationService(
        BillabilityDeterminationService billabilityService,
        ChargeCompositionPipeline chargeCompositionPipeline,
        BillPersistenceCapability billPersistenceCapability,
        MonthlyBillFactory billFactory,
        IBillNumberGenerator billNumberGenerator)
    {
        _billabilityService = billabilityService ?? throw new ArgumentNullException(nameof(billabilityService));
        _chargeCompositionPipeline = chargeCompositionPipeline ?? throw new ArgumentNullException(nameof(chargeCompositionPipeline));
        _billPersistenceCapability = billPersistenceCapability ?? throw new ArgumentNullException(nameof(billPersistenceCapability));
        _billFactory = billFactory ?? throw new ArgumentNullException(nameof(billFactory));
        _billNumberGenerator = billNumberGenerator ?? throw new ArgumentNullException(nameof(billNumberGenerator));
    }

    public MonthlyBillingResult Execute(MonthlyBillingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var billabilityRequest = new BillabilityResolutionRequest(
            request.BillingContext.BillingPeriod,
            request.CandidateProjections);

        var billabilityResolution = _billabilityService.Determine(billabilityRequest);

        var compositionRequest = new ChargeCompositionRequest(
            request.BillingContext,
            billabilityResolution);

        var compositionResult = _chargeCompositionPipeline.Compose(compositionRequest);

        var chargesByCandidate = BuildChargeLookup(compositionResult.ChargeCandidates);
        var generatedBills = new List<GeneratedBillReference>();
        var generatedAggregates = new List<BillAggregate>();
        var nextSequence = 1;

        foreach (var candidate in billabilityResolution.IncludedCandidates)
        {
            var candidateKey = TryCreateCandidateKey(candidate);
            if (candidateKey is null)
            {
                continue;
            }

            if (!chargesByCandidate.TryGetValue(candidateKey.Value, out var charges) || charges.Count == 0)
            {
                continue;
            }

            var billNumber = _billNumberGenerator.Generate(request, candidate, nextSequence++);

            var generateBillCommand = CreateGenerateBillCommand(
                request,
                candidate,
                charges,
                billNumber);

            var generatedBill = _billFactory.Generate(generateBillCommand);
            generatedAggregates.Add(generatedBill);

            generatedBills.Add(new GeneratedBillReference(
                generatedBill.Id,
                generatedBill.BillNumber,
                generateBillCommand.TenancyReference,
                generateBillCommand.LeaseReference,
                generateBillCommand.PropertyReference,
                candidate.UnitId!.Value,
                generatedBill.CurrentSnapshot.TotalAmount.Value,
                generatedBill.CurrentSnapshot.OutstandingAmount.Value));
        }

        var persistenceResult = _billPersistenceCapability.Persist(new BillPersistenceRequest(generatedAggregates));
        var skippedCount = Math.Max(0, billabilityResolution.TotalIncluded - persistenceResult.PersistedCount);

        var summary = new MonthlyBillingSummary(
            billabilityResolution.TotalEvaluated,
            billabilityResolution.TotalIncluded,
            persistenceResult.PersistedCount,
            skippedCount,
            billabilityResolution.TotalExcluded);

        return new MonthlyBillingResult(generatedBills, summary, billabilityResolution);
    }

    private static Dictionary<CandidateKey, List<ChargeCandidate>> BuildChargeLookup(IReadOnlyCollection<ChargeCandidate> charges)
    {
        var lookup = new Dictionary<CandidateKey, List<ChargeCandidate>>();

        foreach (var charge in charges)
        {
            var key = TryCreateCandidateKey(charge);
            if (key is null)
            {
                continue;
            }

            if (!lookup.TryGetValue(key.Value, out var candidateCharges))
            {
                candidateCharges = [];
                lookup[key.Value] = candidateCharges;
            }

            candidateCharges.Add(charge);
        }

        return lookup;
    }

    private static GenerateBillCommand CreateGenerateBillCommand(
        MonthlyBillingRequest request,
        BillabilityCandidate candidate,
        IReadOnlyCollection<ChargeCandidate> charges,
        BillNumber billNumber)
    {
        var currency = ResolveSingleCurrency(charges);
        var chargeCollection = ChargeCollection.Create(charges.Select(ToChargeLine));

        return new GenerateBillCommand(
            billNumber,
            candidate.TenancyReference!,
            candidate.LeaseReference!,
            candidate.PropertyReference!,
            candidate.PrimaryOccupantReference!,
            request.BillingContext.BillingPeriod,
            request.BillingContext.BillingCycle,
            GeneratedDate.Create(request.GeneratedDate),
            IssueDate.Create(request.IssueDate),
            DueDate.Create(request.DueDate),
            currency,
            chargeCollection);
    }

    private static Currency ResolveSingleCurrency(IReadOnlyCollection<ChargeCandidate> charges)
    {
        ArgumentNullException.ThrowIfNull(charges);

        var currencies = charges
            .Select(x => x.Currency.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (currencies.Count != 1)
        {
            throw new InvalidOperationException("Exactly one currency is required per bill snapshot.");
        }

        return Currency.Create(currencies[0]);
    }

    private static ChargeLine ToChargeLine(ChargeCandidate candidate)
    {
        return ChargeLine.Create(
            ChargeKind.Create(candidate.ChargeType),
            candidate.Description,
            candidate.Amount,
            candidate.ExternalReference);
    }

    private static CandidateKey? TryCreateCandidateKey(BillabilityCandidate candidate)
    {
        if (candidate.TenancyReference is null ||
            candidate.LeaseReference is null ||
            candidate.PropertyReference is null)
        {
            return null;
        }

        return new CandidateKey(
            candidate.TenancyReference.TenancyId,
            candidate.LeaseReference.LeaseId,
            candidate.PropertyReference.PropertyId);
    }

    private static CandidateKey? TryCreateCandidateKey(ChargeCandidate candidate)
    {
        if (!TryReadMetadataGuid(candidate, "TenancyId", out var tenancyId) ||
            !TryReadMetadataGuid(candidate, "LeaseId", out var leaseId) ||
            !TryReadMetadataGuid(candidate, "PropertyId", out var propertyId))
        {
            return null;
        }

        return new CandidateKey(tenancyId, leaseId, propertyId);
    }

    private static bool TryReadMetadataGuid(ChargeCandidate candidate, string key, out Guid value)
    {
        value = Guid.Empty;

        if (!candidate.Metadata.TryGetValue(key, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        return Guid.TryParse(rawValue, out value);
    }

    private readonly record struct CandidateKey(Guid TenancyId, Guid LeaseId, Guid PropertyId);
}
