using Masterdom.Modules.Billing.Contracts.Published.Models;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Publication;

public class BillingSnapshotProjector
{
    public virtual BillSnapshotModel Project(BillAggregate bill, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(bill);

        var snapshot = bill.CurrentSnapshot;

        return new BillSnapshotModel(
            bill.Id.Value,
            bill.BillNumber.Value,
            snapshot.BillingPeriod.StartDate,
            snapshot.BillingPeriod.EndDate,
            bill.PropertyReference.PropertyId,
            bill.TenancyReference.TenancyId,
            bill.LeaseReference.LeaseId,
            snapshot.IssueDate.Value,
            snapshot.DueDate.Value,
            snapshot.Currency.Code,
            snapshot.TotalAmount.Value,
            snapshot.OutstandingAmount.Value,
            snapshot.Charges.Items
                .Select(x => new BillSnapshotChargeLineModel(
                    x.Kind.Value,
                    x.Description,
                    x.Amount,
                    x.ExternalReference))
                .ToList(),
            snapshot.GeneratedDate.Value,
            correlationId);
    }
}
