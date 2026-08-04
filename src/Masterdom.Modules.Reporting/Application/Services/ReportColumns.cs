using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

internal static class ReportColumns
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<ReportColumn>> Map =
        new Dictionary<string, IReadOnlyCollection<ReportColumn>>(StringComparer.OrdinalIgnoreCase)
        {
            ["active-tenancies"] = [new("tenancyId", "Tenancy Id"), new("status", "Status"), new("occupancy", "Occupancy")],
            ["vacant-units"] = [new("unitId", "Unit Id"), new("status", "Status"), new("unitType", "Unit Type")],
            ["occupancy-summary"] = [new("propertyId", "Property Id"), new("totalUnits", "Total Units"), new("occupiedUnits", "Occupied Units"), new("occupancyRate", "Occupancy Rate")],
            ["upcoming-move-ins"] = [new("tenancyId", "Tenancy Id"), new("moveInDate", "Move In Date"), new("status", "Status")],
            ["upcoming-move-outs"] = [new("tenancyId", "Tenancy Id"), new("moveOutDate", "Move Out Date"), new("status", "Status")],
            ["meter-reading-history"] = [new("meterId", "Meter Id"), new("readingDate", "Reading Date"), new("readingValue", "Reading Value"), new("approvalStatus", "Approval Status")],
            ["missing-readings"] = [new("meterId", "Meter Id"), new("lastReadingDate", "Last Reading Date")],
            ["consumption-summary"] = [new("meterId", "Meter Id"), new("totalConsumption", "Total Consumption")],
            ["high-consumption"] = [new("meterId", "Meter Id"), new("consumption", "Consumption")],
            ["reading-corrections"] = [new("meterId", "Meter Id"), new("readingId", "Reading Id"), new("correctionCount", "Correction Count")],
            ["bills-generated"] = [new("billId", "Bill Id"), new("billNumber", "Bill Number"), new("status", "Status")],
            ["bills-pending"] = [new("billId", "Bill Id"), new("billNumber", "Bill Number"), new("status", "Status")],
            ["bills-finalized"] = [new("billId", "Bill Id"), new("billNumber", "Bill Number"), new("status", "Status")],
            ["bills-voided"] = [new("billId", "Bill Id"), new("billNumber", "Bill Number"), new("status", "Status")],
            ["outstanding-bills"] = [new("billId", "Bill Id"), new("billNumber", "Bill Number"), new("outstandingAmount", "Outstanding Amount")],
            ["billing-summary"] = [new("billCount", "Bill Count"), new("totalOutstanding", "Total Outstanding")],
            ["charge-breakdown"] = [new("billId", "Bill Id"), new("chargeTotal", "Charge Total")],
            ["trial-balance"] = [new("accountCode", "Account Code"), new("accountName", "Account Name"), new("debits", "Debits"), new("credits", "Credits")],
            ["general-ledger"] = [new("journalNumber", "Journal Number"), new("postingReference", "Posting Reference"), new("debits", "Debits"), new("credits", "Credits")],
            ["journal-register"] = [new("journalNumber", "Journal Number"), new("description", "Description"), new("sourceModule", "Source Module")],
            ["account-balances"] = [new("accountCode", "Account Code"), new("accountName", "Account Name"), new("balance", "Balance")],
            ["payment-register"] = [new("paymentId", "Payment Id"), new("paymentReference", "Payment Reference"), new("status", "Status"), new("amount", "Amount")],
            ["unallocated-payments"] = [new("paymentId", "Payment Id"), new("paymentReference", "Payment Reference"), new("unallocated", "Unallocated")],
            ["partially-allocated-payments"] = [new("paymentId", "Payment Id"), new("paymentReference", "Payment Reference"), new("allocated", "Allocated"), new("unallocated", "Unallocated")],
            ["payment-reversals"] = [new("paymentId", "Payment Id"), new("paymentReference", "Payment Reference"), new("reversedAt", "Reversed At")],
            ["collection-summary"] = [new("paymentCount", "Payment Count"), new("totalCollections", "Total Collections")],
            ["property-performance"] = [new("propertyId", "Property Id"), new("unitCount", "Unit Count"), new("occupancyRate", "Occupancy Rate"), new("collections", "Collections")],
            ["collection-efficiency"] = [new("totalOutstanding", "Total Outstanding"), new("totalCollected", "Total Collected"), new("efficiencyRate", "Efficiency Rate")],
            ["occupancy-rate"] = [new("occupiedUnits", "Occupied Units"), new("totalUnits", "Total Units"), new("occupancyRate", "Occupancy Rate")],
            ["revenue-summary"] = [new("ledgerCredits", "Ledger Credits"), new("billedAmount", "Billed Amount"), new("collectedAmount", "Collected Amount")],
            ["arrears-summary"] = [new("totalOutstanding", "Total Outstanding"), new("billCount", "Bill Count")],
            ["monthly-dashboard"] = [new("period", "Period"), new("occupancyRate", "Occupancy Rate"), new("collections", "Collections"), new("outstanding", "Outstanding")]
        };
}
