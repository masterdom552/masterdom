using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Additional;

public sealed class AdditionalChargeSource : IChargeSource
{
    public string ProviderId => "Additional";

    public IReadOnlyCollection<ChargeCandidate> Compose(ChargeCompositionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var charges = new List<ChargeCandidate>();

        foreach (var candidate in request.BillabilityResolutionResult.IncludedCandidates)
        {
            if (candidate.TenancyReference is null ||
                candidate.LeaseReference is null ||
                candidate.PropertyReference is null ||
                candidate.UnitId is null)
            {
                continue;
            }

            charges.Add(new ChargeCandidate(
                chargeType: "Maintenance",
                description: "Maintenance service charge",
                amount: 50m,
                currency: "USD",
                sourceCapability: "AdditionalCharges",
                externalReference: candidate.LeaseReference.LeaseId.ToString("N"),
                metadata: BuildMetadata(candidate)));
        }

        return charges.AsReadOnly();
    }

    private static IReadOnlyDictionary<string, string> BuildMetadata(Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts.BillabilityCandidate candidate)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TenancyId"] = candidate.TenancyReference!.TenancyId.ToString("D"),
            ["LeaseId"] = candidate.LeaseReference!.LeaseId.ToString("D"),
            ["PropertyId"] = candidate.PropertyReference!.PropertyId.ToString("D")
        };
    }
}
