using Masterdom.Modules.Billing.Contracts.Published.Notifications;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Publication;

public class BillingNotificationProjector
{
    public virtual BillPersistedNotification ProjectBillPersisted(
        string correlationId,
        IReadOnlyCollection<BillAggregate> persistedBills,
        DateTime executionTimestampUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentNullException.ThrowIfNull(persistedBills);

        var bills = persistedBills.ToList();
        if (bills.Count == 0)
        {
            throw new ArgumentException("At least one persisted bill is required.", nameof(persistedBills));
        }

        var firstBill = bills[0];
        var distinctPropertyIds = bills
            .Select(x => x.PropertyReference.PropertyId)
            .Distinct()
            .ToList();

        return new BillPersistedNotification(
            correlationId,
            firstBill.CurrentSnapshot.BillingPeriod.StartDate,
            firstBill.CurrentSnapshot.BillingPeriod.EndDate,
            bills.Select(x => x.Id.Value).ToList(),
            bills.Count,
            executionTimestampUtc,
            distinctPropertyIds.Count == 1 ? distinctPropertyIds[0] : null);
    }
}
