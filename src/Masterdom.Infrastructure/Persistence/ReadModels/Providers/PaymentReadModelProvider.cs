using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.ReadModels.Providers;

internal sealed class PaymentReadModelProvider : IPaymentReadModelProvider
{
    private readonly MasterdomDbContext _dbContext;

    public PaymentReadModelProvider(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public string ModuleId => "payment";

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
    [
        new(ModuleId, BaselineReadModelKeys.PaymentRegister, 1, "Payment register rows.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["paymentId"] = "string", ["paymentReference"] = "string", ["status"] = "string", ["amount"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.UnallocatedPayments, 1, "Unallocated payments.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId"], new Dictionary<string, string> { ["paymentId"] = "string", ["paymentReference"] = "string", ["unallocated"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.PartiallyAllocatedPayments, 1, "Partially allocated payments.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId"], new Dictionary<string, string> { ["paymentId"] = "string", ["paymentReference"] = "string", ["allocated"] = "string", ["unallocated"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.PaymentReversals, 1, "Payment reversals.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate"], new Dictionary<string, string> { ["paymentId"] = "string", ["paymentReference"] = "string", ["reversedAt"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.CollectionSummary, 1, "Collection summary totals.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["fromDate", "toDate"], new Dictionary<string, string> { ["paymentCount"] = "string", ["totalCollections"] = "string" }),
        new(ModuleId, BaselineReadModelKeys.CollectionsByProperty, 1, "Collection totals by property via bill allocations.", nameof(PaymentReadModelProvider), ["Reporting", "Dashboards", "Analytics", "Notifications", "Search", "AI", "Public APIs", "Data Export"], ["propertyId", "fromDate", "toDate"], new Dictionary<string, string> { ["propertyId"] = "string", ["collections"] = "string" })
    ];

    public IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        var payments = _dbContext.Payments.AsNoTracking().ToList();

        return readModelKey switch
        {
            BaselineReadModelKeys.PaymentRegister => payments
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["paymentId"] = x.Id.Value.ToString("N"),
                    ["paymentReference"] = x.PaymentReference.Value,
                    ["status"] = x.PaymentStatus.Value,
                    ["amount"] = x.PaymentAmount.Value.ToString("0.##")
                }))
                .ToList(),

            BaselineReadModelKeys.UnallocatedPayments => payments
                .Select(x => new { payment = x, allocated = x.Allocations.Where(a => !a.IsReversed).Sum(a => a.Amount.Value) })
                .Where(x => x.allocated == 0m)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["paymentId"] = x.payment.Id.Value.ToString("N"),
                    ["paymentReference"] = x.payment.PaymentReference.Value,
                    ["unallocated"] = x.payment.PaymentAmount.Value.ToString("0.##")
                }))
                .ToList(),

            BaselineReadModelKeys.PartiallyAllocatedPayments => payments
                .Select(x => new { payment = x, allocated = x.Allocations.Where(a => !a.IsReversed).Sum(a => a.Amount.Value) })
                .Where(x => x.allocated > 0m && x.allocated < x.payment.PaymentAmount.Value)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["paymentId"] = x.payment.Id.Value.ToString("N"),
                    ["paymentReference"] = x.payment.PaymentReference.Value,
                    ["allocated"] = x.allocated.ToString("0.##"),
                    ["unallocated"] = (x.payment.PaymentAmount.Value - x.allocated).ToString("0.##")
                }))
                .ToList(),

            BaselineReadModelKeys.PaymentReversals => payments
                .Where(x => x.ReversedAtUtc != null)
                .Select(x => new ReadModelRecord(new Dictionary<string, string>
                {
                    ["paymentId"] = x.Id.Value.ToString("N"),
                    ["paymentReference"] = x.PaymentReference.Value,
                    ["reversedAt"] = x.ReversedAtUtc!.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }))
                .ToList(),

            BaselineReadModelKeys.CollectionSummary =>
            [
                new ReadModelRecord(new Dictionary<string, string>
                {
                    ["paymentCount"] = payments.Count.ToString(),
                    ["totalCollections"] = payments.Sum(x => x.PaymentAmount.Value).ToString("0.##")
                })
            ],

            BaselineReadModelKeys.CollectionsByProperty => BuildCollectionsByProperty(payments),

            _ => throw new InvalidOperationException($"Unsupported read model key '{readModelKey}' for payment provider.")
        };
    }

    private IReadOnlyCollection<ReadModelRecord> BuildCollectionsByProperty(
        IReadOnlyCollection<Masterdom.Modules.Payment.Domain.Entities.Payment.Payment> payments)
    {
        var bills = _dbContext.Bills.AsNoTracking().ToList();
        var totals = new Dictionary<Guid, decimal>();

        foreach (var payment in payments)
        {
            foreach (var allocation in payment.Allocations.Where(x => !x.IsReversed))
            {
                var bill = bills.FirstOrDefault(x => x.Id.Value == allocation.BillId);
                if (bill is null)
                {
                    continue;
                }

                if (!totals.ContainsKey(bill.PropertyReference.PropertyId))
                {
                    totals[bill.PropertyReference.PropertyId] = 0m;
                }

                totals[bill.PropertyReference.PropertyId] += allocation.Amount.Value;
            }
        }

        return totals
            .Select(x => new ReadModelRecord(new Dictionary<string, string>
            {
                ["propertyId"] = x.Key.ToString("N"),
                ["collections"] = x.Value.ToString("0.##")
            }))
            .ToList();
    }
}
