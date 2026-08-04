namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingPostingPolicy
{
    private readonly IPostingRuleProvider _ruleEngine;

    public BillingPostingPolicy(IPostingRuleProvider ruleEngine)
    {
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
    }

    public IReadOnlyCollection<BillingAccountingRule> GetRuleCatalog()
    {
        return _ruleEngine.GetRuleCatalog(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public PostingAccountSelection SelectAccounts(string chargeCategory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeCategory);

        return _ruleEngine
            .Resolve(chargeCategory, DateOnly.FromDateTime(DateTime.UtcNow))
            .AccountSelection;
    }
}
