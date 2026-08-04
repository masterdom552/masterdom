namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal interface IPostingRuleProvider
{
    PostingRuleResolution Resolve(string chargeCategory, DateOnly asOfDate);

    IReadOnlyCollection<BillingAccountingRule> GetRuleCatalog(DateOnly asOfDate);
}
