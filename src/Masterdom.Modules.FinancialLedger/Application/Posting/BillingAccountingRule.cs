namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed record BillingAccountingRule(
    string BusinessEvent,
    string SourceBusinessFact,
    string DebitAccountCode,
    string DebitAccountName,
    string CreditAccountCode,
    string CreditAccountName,
    string PostingPolicy,
    string BalancingBehavior);
