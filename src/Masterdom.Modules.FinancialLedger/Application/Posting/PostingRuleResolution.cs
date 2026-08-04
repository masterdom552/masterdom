namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PostingRuleResolution
{
    public PostingRuleResolution(
        string chargeCategory,
        PostingRuleDefinition rule,
        PostingAccountSelection accountSelection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeCategory);
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        AccountSelection = accountSelection ?? throw new ArgumentNullException(nameof(accountSelection));

        ChargeCategory = chargeCategory.Trim().ToUpperInvariant();
    }

    public string ChargeCategory { get; }

    public PostingRuleDefinition Rule { get; }

    public PostingAccountSelection AccountSelection { get; }
}
