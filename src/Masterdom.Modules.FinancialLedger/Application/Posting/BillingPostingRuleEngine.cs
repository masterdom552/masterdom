namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingPostingRuleEngine : IPostingRuleProvider
{
    private readonly IChartOfAccounts _chartOfAccounts;
    private readonly BillingPostingRuleEngineOptions _options;

    public BillingPostingRuleEngine(
        IChartOfAccounts chartOfAccounts,
        BillingPostingRuleEngineOptions options)
    {
        _chartOfAccounts = chartOfAccounts ?? throw new ArgumentNullException(nameof(chartOfAccounts));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public PostingRuleResolution Resolve(string chargeCategory, DateOnly asOfDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeCategory);

        var normalizedCategory = chargeCategory.Trim().ToUpperInvariant();
        var rule = _options.RulesByChargeCategory.TryGetValue(normalizedCategory, out var resolvedRule)
            ? resolvedRule
            : _options.FallbackRule;

        var debitAccount = _chartOfAccounts.ResolveRequiredAccount(rule.DebitAccountCode, asOfDate);
        var creditAccount = _chartOfAccounts.ResolveRequiredAccount(rule.CreditAccountCode, asOfDate);

        var selection = new PostingAccountSelection(
            debitAccount.AccountCode,
            debitAccount.AccountName,
            creditAccount.AccountCode,
            creditAccount.AccountName);

        return new PostingRuleResolution(normalizedCategory, rule, selection);
    }

    public IReadOnlyCollection<BillingAccountingRule> GetRuleCatalog(DateOnly asOfDate)
    {
        var rules = _options.RulesByChargeCategory.Values
            .Concat([_options.FallbackRule])
            .DistinctBy(x => x.RuleCode)
            .Select(rule =>
            {
                var debit = _chartOfAccounts.ResolveRequiredAccount(rule.DebitAccountCode, asOfDate);
                var credit = _chartOfAccounts.ResolveRequiredAccount(rule.CreditAccountCode, asOfDate);

                return new BillingAccountingRule(
                    rule.BusinessEvent,
                    rule.SourceBusinessFact,
                    debit.AccountCode,
                    debit.AccountName,
                    credit.AccountCode,
                    credit.AccountName,
                    rule.PostingPolicy,
                    rule.BalancingBehavior);
            })
            .ToList();

        return rules.AsReadOnly();
    }
}
