using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class ReportReadModelRegistry : IReportReadModelRegistry
{
    private static readonly IReadOnlyCollection<ReportReadModelRegistration> Registrations =
    [
        new("active-tenancies", [BaselineReadModelKeys.ActiveTenancies], ["propertyId", "status"], ToSchema(["tenancyId", "status", "occupancy"]), "Active tenancies report."),
        new("vacant-units", [BaselineReadModelKeys.VacantUnits], ["propertyId", "unitType"], ToSchema(["unitId", "status", "unitType"]), "Vacant units report."),
        new("occupancy-summary", [BaselineReadModelKeys.OccupancySummary], ["propertyId"], ToSchema(["propertyId", "totalUnits", "occupiedUnits", "occupancyRate"]), "Occupancy summary report."),
        new("upcoming-move-ins", [BaselineReadModelKeys.UpcomingMoveIns], ["fromDate", "toDate"], ToSchema(["tenancyId", "moveInDate", "status"]), "Upcoming move-ins report."),
        new("upcoming-move-outs", [BaselineReadModelKeys.UpcomingMoveOuts], ["fromDate", "toDate"], ToSchema(["tenancyId", "moveOutDate", "status"]), "Upcoming move-outs report."),
        new("meter-reading-history", [BaselineReadModelKeys.MeterReadingHistory], ["meterId", "fromDate", "toDate"], ToSchema(["meterId", "readingDate", "readingValue", "approvalStatus"]), "Meter reading history report."),
        new("missing-readings", [BaselineReadModelKeys.MissingReadings], ["propertyId"], ToSchema(["meterId", "lastReadingDate"]), "Missing readings report."),
        new("consumption-summary", [BaselineReadModelKeys.ConsumptionSummary], ["meterId"], ToSchema(["meterId", "totalConsumption"]), "Consumption summary report."),
        new("high-consumption", [BaselineReadModelKeys.HighConsumption], ["threshold"], ToSchema(["meterId", "consumption"]), "High consumption report."),
        new("reading-corrections", [BaselineReadModelKeys.ReadingCorrections], ["meterId"], ToSchema(["meterId", "readingId", "correctionCount"]), "Reading corrections report."),
        new("bills-generated", [BaselineReadModelKeys.BillsGenerated], ["propertyId", "fromDate", "toDate"], ToSchema(["billId", "billNumber", "status"]), "Generated bills report."),
        new("bills-pending", [BaselineReadModelKeys.BillsPending], ["propertyId"], ToSchema(["billId", "billNumber", "status"]), "Pending bills report."),
        new("bills-finalized", [BaselineReadModelKeys.BillsFinalized], ["propertyId", "fromDate", "toDate"], ToSchema(["billId", "billNumber", "status"]), "Finalized bills report."),
        new("bills-voided", [BaselineReadModelKeys.BillsVoided], ["propertyId", "fromDate", "toDate"], ToSchema(["billId", "billNumber", "status"]), "Voided bills report."),
        new("outstanding-bills", [BaselineReadModelKeys.OutstandingBills], ["propertyId"], ToSchema(["billId", "billNumber", "outstandingAmount"]), "Outstanding bills report."),
        new("billing-summary", [BaselineReadModelKeys.BillingSummary], ["propertyId", "fromDate", "toDate"], ToSchema(["billCount", "totalOutstanding"]), "Billing summary report."),
        new("charge-breakdown", [BaselineReadModelKeys.ChargeBreakdown], ["propertyId", "fromDate", "toDate"], ToSchema(["billId", "chargeTotal"]), "Charge breakdown report."),
        new("trial-balance", [BaselineReadModelKeys.TrialBalance], ["fromDate", "toDate"], ToSchema(["accountCode", "accountName", "debits", "credits"]), "Trial balance report."),
        new("general-ledger", [BaselineReadModelKeys.GeneralLedger], ["fromDate", "toDate", "accountCode"], ToSchema(["journalNumber", "postingReference", "debits", "credits"]), "General ledger report."),
        new("journal-register", [BaselineReadModelKeys.JournalRegister], ["fromDate", "toDate"], ToSchema(["journalNumber", "description", "sourceModule"]), "Journal register report."),
        new("account-balances", [BaselineReadModelKeys.AccountBalances], ["accountCode"], ToSchema(["accountCode", "accountName", "balance"]), "Account balances report."),
        new("payment-register", [BaselineReadModelKeys.PaymentRegister], ["propertyId", "fromDate", "toDate"], ToSchema(["paymentId", "paymentReference", "status", "amount"]), "Payment register report."),
        new("unallocated-payments", [BaselineReadModelKeys.UnallocatedPayments], ["propertyId"], ToSchema(["paymentId", "paymentReference", "unallocated"]), "Unallocated payments report."),
        new("partially-allocated-payments", [BaselineReadModelKeys.PartiallyAllocatedPayments], ["propertyId"], ToSchema(["paymentId", "paymentReference", "allocated", "unallocated"]), "Partially allocated payments report."),
        new("payment-reversals", [BaselineReadModelKeys.PaymentReversals], ["fromDate", "toDate"], ToSchema(["paymentId", "paymentReference", "reversedAt"]), "Payment reversals report."),
        new("collection-summary", [BaselineReadModelKeys.CollectionSummary], ["fromDate", "toDate"], ToSchema(["paymentCount", "totalCollections"]), "Collection summary report."),
        new("property-performance", [BaselineReadModelKeys.OccupancySummary, BaselineReadModelKeys.CollectionsByProperty], ["propertyId", "fromDate", "toDate"], ToSchema(["propertyId", "unitCount", "occupancyRate", "collections"]), "Property performance report."),
        new("collection-efficiency", [BaselineReadModelKeys.OutstandingBills, BaselineReadModelKeys.CollectionSummary], ["fromDate", "toDate"], ToSchema(["totalOutstanding", "totalCollected", "efficiencyRate"]), "Collection efficiency report."),
        new("occupancy-rate", [BaselineReadModelKeys.OccupancySummary], ["propertyId"], ToSchema(["occupiedUnits", "totalUnits", "occupancyRate"]), "Occupancy rate report."),
        new("revenue-summary", [BaselineReadModelKeys.LedgerCreditSummary, BaselineReadModelKeys.BillingSummary, BaselineReadModelKeys.CollectionSummary], ["fromDate", "toDate"], ToSchema(["ledgerCredits", "billedAmount", "collectedAmount"]), "Revenue summary report."),
        new("arrears-summary", [BaselineReadModelKeys.BillingSummary], ["propertyId"], ToSchema(["totalOutstanding", "billCount"]), "Arrears summary report."),
        new("monthly-dashboard", [BaselineReadModelKeys.OccupancySummary, BaselineReadModelKeys.CollectionSummary, BaselineReadModelKeys.BillingSummary], ["period"], ToSchema(["period", "occupancyRate", "collections", "outstanding"]), "Monthly dashboard report.")
    ];

    public IReadOnlyCollection<ReportReadModelRegistration> GetRegistrations() => Registrations;

    public ReportReadModelRegistration Resolve(string reportCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);

        var registration = Registrations.FirstOrDefault(x =>
            x.ReportCode.Equals(reportCode, StringComparison.OrdinalIgnoreCase));

        return registration ?? throw new InvalidOperationException($"No read model registration exists for report '{reportCode}'.");
    }

    private static IReadOnlyDictionary<string, string> ToSchema(IEnumerable<string> keys)
    {
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            dictionary[key] = "string";
        }

        return dictionary;
    }
}
