using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class BillingReadModelProvider : IBillingReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public BillingReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "billing";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(ModuleId, BaselineReadModelKeys.BillsGenerated, 1, "Generated bills.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["billId"] = "string", ["billNumber"] = "string", ["status"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.BillsPending, 1, "Pending bills.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId"], new Dictionary<string, string> { ["billId"] = "string", ["billNumber"] = "string", ["status"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.BillsFinalized, 1, "Finalized bills.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["billId"] = "string", ["billNumber"] = "string", ["status"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.BillsVoided, 1, "Voided bills.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["billId"] = "string", ["billNumber"] = "string", ["status"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.OutstandingBills, 1, "Outstanding bills.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId"], new Dictionary<string, string> { ["billId"] = "string", ["billNumber"] = "string", ["outstandingAmount"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.BillingSummary, 1, "Billing summary totals.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["billCount"] = "string", ["totalOutstanding"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.ChargeBreakdown, 1, "Bill charge totals.", nameof(BillingReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["billId"] = "string", ["chargeTotal"] = "string" })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var bills = _dbContext.Bills.AsNoTracking().ToList();

        return readModelKey switch
        {
            BaselineReadModelKeys.BillsGenerated => BillsByStatus(bills, "Generated"),
            BaselineReadModelKeys.BillsPending => BillsByStatus(bills, "Draft"),
            BaselineReadModelKeys.BillsFinalized => BillsByStatus(bills, "Finalized"),
            BaselineReadModelKeys.BillsVoided => BillsByStatus(bills, "Voided"),
            BaselineReadModelKeys.OutstandingBills => bills
                .Where(x => x.CurrentSnapshot.OutstandingAmount.Value > 0m)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["billId"] = x.Id.Value.ToString("N"),
                    ["billNumber"] = x.BillNumber.Value,
                    ["outstandingAmount"] = x.CurrentSnapshot.OutstandingAmount.Value.ToString("0.##")
                }))
                .ToList(),
            BaselineReadModelKeys.BillingSummary =>
            [
                new ReadModelRecord(new Dictionary<string, string>
                {
                    ["billCount"] = bills.Count.ToString(),
                    ["totalOutstanding"] = bills.Sum(x => x.CurrentSnapshot.OutstandingAmount.Value).ToString("0.##")
                })
            ],
            BaselineReadModelKeys.ChargeBreakdown => bills
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["billId"] = x.Id.Value.ToString("N"),
                    ["chargeTotal"] = x.CurrentSnapshot.Charges.TotalAmount.ToString("0.##")
                }))
                .ToList(),
            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for billing provider.")
        };
    }

    private static IReadOnlyCollection<ReadModelRecord> BillsByStatus(
        IReadOnlyCollection<Masterdom.Modules.Billing.Domain.Entities.Billing.Bill> bills,
        string status)
    {
        return bills
            .Where(x => x.Status.Value == status)
            .Select(x => new ReadModelRecord(new Dictionary<string, string>
            {
                ["billId"] = x.Id.Value.ToString("N"),
                ["billNumber"] = x.BillNumber.Value,
                ["status"] = x.Status.Value
            }))
            .ToList();
    }
}
