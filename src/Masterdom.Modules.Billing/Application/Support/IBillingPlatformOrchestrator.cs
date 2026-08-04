using Masterdom.Modules.Billing.Application.Events;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Support;

/// <summary>
/// Coordinates platform framework interactions for billing operations.
/// </summary>
public interface IBillingPlatformOrchestrator
{
    void OnBillMutated(BillAggregate bill, string operationName);

    void Publish(IBillingApplicationEvent applicationEvent);
}
