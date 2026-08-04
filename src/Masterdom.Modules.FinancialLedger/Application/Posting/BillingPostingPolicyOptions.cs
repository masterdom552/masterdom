namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingPostingPolicyOptions
{
    public string ReceivableAccountCode { get; init; } = "1100";

    public string ReceivableAccountName { get; init; } = "Accounts Receivable";

    public IReadOnlyDictionary<string, (string AccountCode, string AccountName)> CreditAccountsByCategory { get; init; }
        = new Dictionary<string, (string AccountCode, string AccountName)>(StringComparer.OrdinalIgnoreCase)
        {
            ["RENT"] = ("4100", "Rental Revenue"),
            ["UTILITYREFERENCE"] = ("4200", "Utility Revenue"),
            ["MAINTENANCE"] = ("4300", "Maintenance Recovery Revenue"),
            ["RECURRING"] = ("4400", "Recurring Revenue"),
            ["ONETIME"] = ("4500", "One-Time Revenue"),
            ["CARRYFORWARD"] = ("4600", "Carry Forward Revenue")
        };

    public string FallbackCreditAccountCode { get; init; } = "4999";

    public string FallbackCreditAccountName { get; init; } = "Other Billing Revenue";
}
