using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;

public sealed class MonthlyBillingResult
{
    public MonthlyBillingResult(
        IReadOnlyCollection<GeneratedBillReference> generatedBills,
        MonthlyBillingSummary summary,
        BillabilityResolutionResult billabilityResolution)
    {
        ArgumentNullException.ThrowIfNull(generatedBills);
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(billabilityResolution);

        GeneratedBills = generatedBills.ToList().AsReadOnly();
        Summary = summary;
        BillabilityResolution = billabilityResolution;
    }

    public IReadOnlyCollection<GeneratedBillReference> GeneratedBills { get; }

    public MonthlyBillingSummary Summary { get; }

    public BillabilityResolutionResult BillabilityResolution { get; }
}
