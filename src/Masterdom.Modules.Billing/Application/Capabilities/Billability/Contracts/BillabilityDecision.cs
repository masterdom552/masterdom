namespace Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

public sealed record BillabilityDecision(
    bool IsBillable,
    IReadOnlyCollection<BillabilityExclusionReason> Reasons,
    IReadOnlyCollection<string> Warnings)
{
    public static BillabilityDecision Billable()
    {
        return new BillabilityDecision(true, Array.Empty<BillabilityExclusionReason>(), Array.Empty<string>());
    }

    public static BillabilityDecision Excluded(IReadOnlyCollection<BillabilityExclusionReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);

        return new BillabilityDecision(false, reasons, Array.Empty<string>());
    }
}
