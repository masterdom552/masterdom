namespace Masterdom.Modules.Reporting.Application.Services;

internal static class ReportCatalog
{
    public static readonly IReadOnlyCollection<string> Codes =
    [
        "active-tenancies",
        "vacant-units",
        "occupancy-summary",
        "upcoming-move-ins",
        "upcoming-move-outs",
        "meter-reading-history",
        "missing-readings",
        "consumption-summary",
        "high-consumption",
        "reading-corrections",
        "bills-generated",
        "bills-pending",
        "bills-finalized",
        "bills-voided",
        "outstanding-bills",
        "billing-summary",
        "charge-breakdown",
        "trial-balance",
        "general-ledger",
        "journal-register",
        "account-balances",
        "payment-register",
        "unallocated-payments",
        "partially-allocated-payments",
        "payment-reversals",
        "collection-summary",
        "property-performance",
        "collection-efficiency",
        "occupancy-rate",
        "revenue-summary",
        "arrears-summary",
        "monthly-dashboard"
    ];
}
