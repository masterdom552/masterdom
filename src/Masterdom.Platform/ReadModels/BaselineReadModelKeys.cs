namespace Masterdom.Platform.ReadModels;

public static class BaselineReadModelKeys
{
    public const string ActiveTenancies = "tenancy.active-tenancies.v1";
    public const string UpcomingMoveIns = "tenancy.upcoming-move-ins.v1";
    public const string UpcomingMoveOuts = "tenancy.upcoming-move-outs.v1";

    public const string VacantUnits = "property.vacant-units.v1";
    public const string OccupancySummary = "property.occupancy-summary.v1";

    public const string MeterReadingHistory = "metering.meter-reading-history.v1";
    public const string MissingReadings = "metering.missing-readings.v1";
    public const string ConsumptionSummary = "metering.consumption-summary.v1";
    public const string HighConsumption = "metering.high-consumption.v1";
    public const string ReadingCorrections = "metering.reading-corrections.v1";

    public const string BillsGenerated = "billing.bills-generated.v1";
    public const string BillsPending = "billing.bills-pending.v1";
    public const string BillsFinalized = "billing.bills-finalized.v1";
    public const string BillsVoided = "billing.bills-voided.v1";
    public const string OutstandingBills = "billing.outstanding-bills.v1";
    public const string BillingSummary = "billing.summary.v1";
    public const string ChargeBreakdown = "billing.charge-breakdown.v1";

    public const string TrialBalance = "financial-ledger.trial-balance.v1";
    public const string GeneralLedger = "financial-ledger.general-ledger.v1";
    public const string JournalRegister = "financial-ledger.journal-register.v1";
    public const string AccountBalances = "financial-ledger.account-balances.v1";
    public const string LedgerCreditSummary = "financial-ledger.credit-summary.v1";

    public const string PaymentRegister = "payment.payment-register.v1";
    public const string UnallocatedPayments = "payment.unallocated-payments.v1";
    public const string PartiallyAllocatedPayments = "payment.partially-allocated-payments.v1";
    public const string PaymentReversals = "payment.payment-reversals.v1";
    public const string CollectionSummary = "payment.collection-summary.v1";
    public const string CollectionsByProperty = "payment.collections-by-property.v1";
}
