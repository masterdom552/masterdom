using Masterdom.Modules.FinancialLedger.Application.Posting;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class InMemoryChartOfAccountsTests
{
    [Fact]
    public void ResolveRequiredAccount_ShouldReturnActiveAccountWithinEffectiveRange()
    {
        var chart = new InMemoryChartOfAccounts(new ChartOfAccountsOptions
        {
            Accounts =
            [
                new ChartOfAccountsEntry("1100", "Accounts Receivable", ChartOfAccountsClassification.Asset, new DateOnly(2020, 1, 1), null, true),
                new ChartOfAccountsEntry("4100", "Rental Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true, "4000")
            ]
        });

        var account = chart.ResolveRequiredAccount("4100", new DateOnly(2026, 8, 31));

        Assert.Equal("4100", account.AccountCode);
        Assert.Equal("4000", account.ParentAccountCode);
    }

    [Fact]
    public void ResolveRequiredAccount_ShouldRejectInactiveOrOutOfRangeAccount()
    {
        var chart = new InMemoryChartOfAccounts(new ChartOfAccountsOptions
        {
            Accounts =
            [
                new ChartOfAccountsEntry("4999", "Other Billing Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), new DateOnly(2025, 12, 31), true),
                new ChartOfAccountsEntry("5100", "Deprecated", ChartOfAccountsClassification.Expense, new DateOnly(2020, 1, 1), null, false)
            ]
        });

        Assert.Throws<InvalidOperationException>(() => chart.ResolveRequiredAccount("4999", new DateOnly(2026, 1, 1)));
        Assert.Throws<InvalidOperationException>(() => chart.ResolveRequiredAccount("5100", new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Constructor_ShouldRejectDuplicateAccountCodes()
    {
        var options = new ChartOfAccountsOptions
        {
            Accounts =
            [
                new ChartOfAccountsEntry("4100", "Rental Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true),
                new ChartOfAccountsEntry("4100", "Duplicate Revenue", ChartOfAccountsClassification.Revenue, new DateOnly(2020, 1, 1), null, true)
            ]
        };

        Assert.Throws<InvalidOperationException>(() => new InMemoryChartOfAccounts(options));
    }
}
