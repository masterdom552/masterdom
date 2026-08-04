using Masterdom.Modules.Billing.Contracts.Published.Models;

namespace Masterdom.Modules.FinancialLedger.Application.Translation;

public sealed class BillingSnapshotTranslator
{
    public BillingSnapshotPostingSourceModel Translate(BillSnapshotModel snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new BillingSnapshotPostingSourceModel(
            snapshot.BillId,
            snapshot.BillNumber,
            snapshot.BillingPeriodStartDate,
            snapshot.BillingPeriodEndDate,
            snapshot.PropertyId,
            snapshot.TenancyId,
            snapshot.LeaseId,
            snapshot.IssueDate,
            snapshot.DueDate,
            snapshot.CurrencyCode,
            snapshot.TotalAmount,
            snapshot.OutstandingAmount,
            snapshot.ChargeLines
                .Select(x => new BillingSnapshotPostingChargeLineModel(
                    x.ChargeCategory,
                    x.Description,
                    x.Amount,
                    snapshot.CurrencyCode,
                    x.ExternalReference))
                .ToList()
                .AsReadOnly(),
            snapshot.GeneratedDate,
            snapshot.CorrelationId);
    }
}
