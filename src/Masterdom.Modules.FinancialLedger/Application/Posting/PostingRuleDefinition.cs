namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PostingRuleDefinition
{
    public PostingRuleDefinition(
        string ruleCode,
        string businessEvent,
        string sourceBusinessFact,
        string debitAccountCode,
        string creditAccountCode,
        string postingPolicy,
        string balancingBehavior)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceBusinessFact);
        ArgumentException.ThrowIfNullOrWhiteSpace(debitAccountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditAccountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingPolicy);
        ArgumentException.ThrowIfNullOrWhiteSpace(balancingBehavior);

        RuleCode = ruleCode.Trim().ToUpperInvariant();
        BusinessEvent = businessEvent.Trim();
        SourceBusinessFact = sourceBusinessFact.Trim();
        DebitAccountCode = debitAccountCode.Trim().ToUpperInvariant();
        CreditAccountCode = creditAccountCode.Trim().ToUpperInvariant();
        PostingPolicy = postingPolicy.Trim();
        BalancingBehavior = balancingBehavior.Trim();
    }

    public string RuleCode { get; }

    public string BusinessEvent { get; }

    public string SourceBusinessFact { get; }

    public string DebitAccountCode { get; }

    public string CreditAccountCode { get; }

    public string PostingPolicy { get; }

    public string BalancingBehavior { get; }
}
