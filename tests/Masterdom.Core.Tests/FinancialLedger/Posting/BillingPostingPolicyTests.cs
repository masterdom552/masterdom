using Masterdom.Modules.FinancialLedger.Application.Posting;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class BillingPostingPolicyTests
{
    [Fact]
    public void GetRuleCatalog_ShouldExposeAccountingInventory()
    {
        var policy = new BillingPostingPolicy(CreateProvider());

        var rules = policy.GetRuleCatalog();

        Assert.NotEmpty(rules);
        Assert.Contains(rules, x => x.BusinessEvent == "Monthly Rent");
        Assert.Contains(rules, x => x.BusinessEvent == "Utility Reference");
        Assert.Contains(rules, x => x.BusinessEvent == "Maintenance Recovery");
        Assert.Contains(rules, x => x.BusinessEvent == "Recurring Charge");
        Assert.Contains(rules, x => x.BusinessEvent == "One-Time Charge");
        Assert.Contains(rules, x => x.BusinessEvent == "Carry Forward");
        Assert.Contains(rules, x => x.BusinessEvent == "Unknown Charge Category Fallback");
    }

    [Theory]
    [InlineData("RENT", "1100", "4100")]
    [InlineData("MAINTENANCE", "1100", "4300")]
    [InlineData("UTILITYREFERENCE", "1100", "4200")]
    [InlineData("RECURRING", "1100", "4400")]
    [InlineData("ONETIME", "1100", "4500")]
    [InlineData("CARRYFORWARD", "1100", "4600")]
    [InlineData("UNKNOWN", "1100", "4999")]
    public void SelectAccounts_ShouldReturnCategoryPolicyMapping(string category, string expectedDebit, string expectedCredit)
    {
        var policy = new BillingPostingPolicy(CreateProvider());

        var selection = policy.SelectAccounts(category);

        Assert.Equal(expectedDebit, selection.DebitAccountCode);
        Assert.Equal(expectedCredit, selection.CreditAccountCode);
    }

    private static IPostingRuleProvider CreateProvider()
    {
        return new BillingPostingRuleEngine(
            new InMemoryChartOfAccounts(new ChartOfAccountsOptions()),
            new BillingPostingRuleEngineOptions());
    }
}
