using Masterdom.Modules.FinancialLedger.Application.Posting;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class BillingPostingRuleEngineTests
{
    [Fact]
    public void Resolve_ShouldMapKnownCategoryThroughRuleAndChartOfAccounts()
    {
        var engine = CreateEngine();

        var resolved = engine.Resolve("rent", new DateOnly(2026, 8, 31));

        Assert.Equal("RENT", resolved.ChargeCategory);
        Assert.Equal("BILLING_RENT", resolved.Rule.RuleCode);
        Assert.Equal("1100", resolved.AccountSelection.DebitAccountCode);
        Assert.Equal("4100", resolved.AccountSelection.CreditAccountCode);
    }

    [Fact]
    public void Resolve_ShouldUseFallbackRule_ForUnknownCategory()
    {
        var engine = CreateEngine();

        var resolved = engine.Resolve("unexpected", new DateOnly(2026, 8, 31));

        Assert.Equal("BILLING_FALLBACK", resolved.Rule.RuleCode);
        Assert.Equal("4999", resolved.AccountSelection.CreditAccountCode);
    }

    [Fact]
    public void GetRuleCatalog_ShouldReturnResolvedAccountNamesFromChartOfAccounts()
    {
        var engine = CreateEngine();

        var rules = engine.GetRuleCatalog(new DateOnly(2026, 8, 31));

        Assert.Contains(rules, x => x.BusinessEvent == "Monthly Rent" && x.CreditAccountName == "Rental Revenue");
        Assert.Contains(rules, x => x.BusinessEvent == "Unknown Charge Category Fallback" && x.CreditAccountName == "Other Billing Revenue");
    }

    private static BillingPostingRuleEngine CreateEngine()
    {
        return new BillingPostingRuleEngine(
            new InMemoryChartOfAccounts(new ChartOfAccountsOptions()),
            new BillingPostingRuleEngineOptions());
    }
}
