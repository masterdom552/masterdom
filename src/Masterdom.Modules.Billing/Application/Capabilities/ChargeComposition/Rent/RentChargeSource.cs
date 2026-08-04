using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Rent;

public sealed class RentChargeSource : IChargeSource
{
    public const string ProviderIdentifier = "Rent";

    private const string RentChargeType = "Rent";
    private const string RentSourceCapability = "Rent";

    private readonly IChargeCompositionReadService _readService;

    public RentChargeSource(IChargeCompositionReadService readService)
    {
        _readService = readService ?? throw new ArgumentNullException(nameof(readService));
    }

    public string ProviderId => ProviderIdentifier;

    public IReadOnlyCollection<ChargeCandidate> Compose(ChargeCompositionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var charges = new List<ChargeCandidate>();

        foreach (var candidate in request.BillabilityResolutionResult.IncludedCandidates)
        {
            if (candidate.TenancyReference is null ||
                candidate.LeaseReference is null ||
                candidate.PropertyReference is null ||
                candidate.UnitId is null ||
                candidate.UnitId == Guid.Empty)
            {
                continue;
            }

            var rentModel = _readService.GetRentChargeReadModel(
                candidate.TenancyReference.TenancyId,
                candidate.LeaseReference.LeaseId,
                candidate.PropertyReference.PropertyId,
                candidate.UnitId.Value);

            if (rentModel is null)
            {
                continue;
            }

            if (!rentModel.IsTenancyActive || !rentModel.IsLeaseActive)
            {
                continue;
            }

            if (rentModel.TenancyId != candidate.TenancyReference.TenancyId ||
                rentModel.LeaseId != candidate.LeaseReference.LeaseId ||
                rentModel.PropertyId != candidate.PropertyReference.PropertyId ||
                rentModel.UnitId != candidate.UnitId.Value)
            {
                continue;
            }

            if (rentModel.RentAmount is null ||
                !IsApplicableBillingFrequency(request, rentModel.BillingFrequency))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(rentModel.Currency))
            {
                continue;
            }

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LeaseId"] = rentModel.LeaseId.ToString("D"),
                ["TenancyId"] = rentModel.TenancyId.ToString("D"),
                ["PropertyId"] = rentModel.PropertyId.ToString("D"),
                ["BillingFrequency"] = rentModel.BillingFrequency!.Trim()
            };

            charges.Add(new ChargeCandidate(
                chargeType: RentChargeType,
                description: "Rent charge",
                amount: rentModel.RentAmount.Value,
                currency: rentModel.Currency,
                sourceCapability: RentSourceCapability,
                externalReference: rentModel.LeaseNumber,
                metadata: metadata));
        }

        return charges.AsReadOnly();
    }

    private static bool IsApplicableBillingFrequency(ChargeCompositionRequest request, string? billingFrequency)
    {
        if (string.IsNullOrWhiteSpace(billingFrequency))
        {
            return false;
        }

        var normalizedFrequency = billingFrequency.Trim().ToUpperInvariant();

        return request.BillingContext.BillingCycle.Value.ToUpperInvariant() switch
        {
            "MONTHLY" => normalizedFrequency == "MONTHLY",
            "QUARTERLY" => normalizedFrequency == "QUARTERLY",
            _ => false
        };
    }
}
