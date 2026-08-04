namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed record PostingAccountSelection(
    string DebitAccountCode,
    string DebitAccountName,
    string CreditAccountCode,
    string CreditAccountName);
