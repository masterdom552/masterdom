namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public sealed record BillabilityResolutionResult(
    IReadOnlyCollection<BillabilityCandidate> IncludedCandidates,
    IReadOnlyCollection<ExcludedBillabilityCandidate> ExcludedCandidates)
{
    public int TotalEvaluated => IncludedCandidates.Count + ExcludedCandidates.Count;

    public int TotalIncluded => IncludedCandidates.Count;

    public int TotalExcluded => ExcludedCandidates.Count;

    public IReadOnlyDictionary<BillabilityExclusionReason, int> ExclusionCountsByReason =>
        ExcludedCandidates
            .SelectMany(x => x.Decision.Reasons)
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());
}
