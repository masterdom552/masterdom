namespace Masterdom.Modules.FinancialLedger.Contracts.Billing;

public sealed record LedgerPostingLineContract(
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string Description);
