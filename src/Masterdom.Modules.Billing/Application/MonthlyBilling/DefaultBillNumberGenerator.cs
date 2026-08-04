using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling;

public sealed class DefaultBillNumberGenerator : IBillNumberGenerator
{
    public BillNumber Generate(MonthlyBillingRequest request, BillabilityCandidate candidate, int sequence)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate.TenancyReference is null)
        {
            throw new InvalidOperationException("Tenancy reference is required to generate bill number.");
        }

        var periodToken = request.BillingContext.BillingPeriod.StartDate.ToString("yyyyMM");
        var tenancyToken = candidate.TenancyReference.TenancyId.ToString("N");

        // TODO(BIL-CAP-003A/BIL-CAP-003B): replace with Billing-owned policy for persistent, idempotent numbering.
        return BillNumber.Create($"MB-{periodToken}-{sequence:D4}-{tenancyToken}");
    }
}
