namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingPostingRuleEngineOptions
{
    public IReadOnlyDictionary<string, PostingRuleDefinition> RulesByChargeCategory { get; init; }
        = new Dictionary<string, PostingRuleDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["RENT"] = new(
                "BILLING_RENT",
                "Monthly Rent",
                "BillSnapshot charge line category RENT",
                "1100",
                "4100",
                "Debit receivable and credit rental revenue by charge amount.",
                "One debit equals one or many credit lines summing to bill total."),
            ["UTILITYREFERENCE"] = new(
                "BILLING_UTILITYREFERENCE",
                "Utility Reference",
                "BillSnapshot charge line category UTILITYREFERENCE",
                "1100",
                "4200",
                "Debit receivable and credit utility revenue by charge amount.",
                "One debit equals one or many credit lines summing to bill total."),
            ["MAINTENANCE"] = new(
                "BILLING_MAINTENANCE",
                "Maintenance Recovery",
                "BillSnapshot charge line category MAINTENANCE",
                "1100",
                "4300",
                "Debit receivable and credit maintenance recovery by charge amount.",
                "One debit equals one or many credit lines summing to bill total."),
            ["RECURRING"] = new(
                "BILLING_RECURRING",
                "Recurring Charge",
                "BillSnapshot charge line category RECURRING",
                "1100",
                "4400",
                "Debit receivable and credit recurring revenue by charge amount.",
                "One debit equals one or many credit lines summing to bill total."),
            ["ONETIME"] = new(
                "BILLING_ONETIME",
                "One-Time Charge",
                "BillSnapshot charge line category ONETIME",
                "1100",
                "4500",
                "Debit receivable and credit one-time revenue by charge amount.",
                "One debit equals one or many credit lines summing to bill total."),
            ["CARRYFORWARD"] = new(
                "BILLING_CARRYFORWARD",
                "Carry Forward",
                "BillSnapshot charge line category CARRYFORWARD",
                "1100",
                "4600",
                "Debit receivable and credit carry-forward revenue by charge amount.",
                "One debit equals one or many credit lines summing to bill total.")
        };

    public PostingRuleDefinition FallbackRule { get; init; } = new(
        "BILLING_FALLBACK",
        "Unknown Charge Category Fallback",
        "BillSnapshot charge categories not currently published by Billing",
        "1100",
        "4999",
        "Debit receivable and credit fallback account until Billing publishes a source-owned category.",
        "One debit equals one or many credit lines summing to bill total.");
}
