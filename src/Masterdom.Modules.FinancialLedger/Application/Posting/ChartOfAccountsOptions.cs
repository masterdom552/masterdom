namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class ChartOfAccountsOptions
{
    public IReadOnlyCollection<ChartOfAccountsEntry> Accounts { get; init; } =
        new List<ChartOfAccountsEntry>
        {
            new("1100", "Accounts Receivable", ChartOfAccountsClassification.Asset, new DateOnly(2020, 1, 1), null, true),
            new("4100", "Rental Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4200", "Utility Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4300", "Maintenance Recovery Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4400", "Recurring Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4500", "One-Time Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4600", "Carry Forward Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
            new("4999", "Other Billing Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true)
        };
}
