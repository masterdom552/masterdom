using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling;

public interface IBillNumberGenerator
{
    BillNumber Generate(MonthlyBillingRequest request, BillabilityCandidate candidate, int sequence);
}
