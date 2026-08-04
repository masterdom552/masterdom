using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

public sealed record ChargeCompositionRequest
{
    public ChargeCompositionRequest(
        BillingContext billingContext,
        BillabilityResolutionResult billabilityResolutionResult)
    {
        BillingContext = billingContext ?? throw new ArgumentNullException(nameof(billingContext));
        BillabilityResolutionResult = billabilityResolutionResult ?? throw new ArgumentNullException(nameof(billabilityResolutionResult));
    }

    public BillingContext BillingContext { get; }

    public BillabilityResolutionResult BillabilityResolutionResult { get; }
}
