namespace Masterdom.Modules.FinancialLedger.Contracts.Payment;

public sealed record PaymentLedgerPostingLineContract(
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string Description);
