namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class InMemoryChartOfAccounts : IChartOfAccounts
{
    private readonly IReadOnlyCollection<ChartOfAccountsEntry> _accounts;

    public InMemoryChartOfAccounts()
        : this(new ChartOfAccountsOptions())
    {
    }

    public InMemoryChartOfAccounts(ChartOfAccountsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var materialized = options.Accounts.ToList();
        if (materialized.Count == 0)
        {
            throw new InvalidOperationException("Chart of accounts must include at least one account.");
        }

        var duplicateCodes = materialized
            .GroupBy(x => x.AccountCode, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateCodes.Count > 0)
        {
            throw new InvalidOperationException($"Chart of accounts includes duplicate account codes: {string.Join(", ", duplicateCodes)}.");
        }

        _accounts = materialized.AsReadOnly();
    }

    public ChartOfAccountsEntry ResolveRequiredAccount(string accountCode, DateOnly asOfDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);

        var normalized = accountCode.Trim().ToUpperInvariant();
        var account = _accounts.FirstOrDefault(x =>
            string.Equals(x.AccountCode, normalized, StringComparison.OrdinalIgnoreCase) &&
            x.IsEffectiveOn(asOfDate));

        return account
            ?? throw new InvalidOperationException($"No active chart-of-accounts entry was found for account '{normalized}' on '{asOfDate:yyyy-MM-dd}'.");
    }

    public IReadOnlyCollection<ChartOfAccountsEntry> GetActiveAccounts(DateOnly asOfDate)
    {
        return _accounts
            .Where(x => x.IsEffectiveOn(asOfDate))
            .OrderBy(x => x.AccountCode, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }
}
