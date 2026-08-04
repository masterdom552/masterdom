using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Shared;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Utility;

public sealed class UtilityChargeSource : IChargeSource
{
    public string ProviderId => "Utility";

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
                chargeType: "UtilityReference",
                description: "Utility charge",
                amount: 25m,
                currency: "USD",
                sourceCapability: "UtilityCharges",
                externalReference: candidate.UnitId.Value.ToString("N"),
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
