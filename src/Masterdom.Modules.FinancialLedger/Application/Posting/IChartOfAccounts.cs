namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal interface IChartOfAccounts
{
    ChartOfAccountsEntry ResolveRequiredAccount(string accountCode, DateOnly asOfDate);

    IReadOnlyCollection<ChartOfAccountsEntry> GetActiveAccounts(DateOnly asOfDate);
}
