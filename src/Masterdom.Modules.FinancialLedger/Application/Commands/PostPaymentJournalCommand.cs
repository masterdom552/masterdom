using Masterdom.Modules.FinancialLedger.Contracts.Payment;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Commands;

public sealed record PostPaymentJournalCommand(
    LedgerId LedgerId,
    PaymentLedgerPostingContract Contract,
    DateTime PostedAtUtc);

public static class PostPaymentJournalCommandFactory
{
    public static PostPaymentJournalCommand Create(
        LedgerId ledgerId,
        string postingReference,
        string journalNumber,
        DateOnly postingDate,
        string description,
        string batchReference,
        IEnumerable<(string AccountCode, string AccountName, decimal DebitAmount, decimal CreditAmount, string Description)> lines,
        DateTime postedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(ledgerId);
        ArgumentNullException.ThrowIfNull(lines);

        var contract = new PaymentLedgerPostingContract(
            postingReference,
            journalNumber,
            postingDate,
            description,
            batchReference,
            lines.Select(x => new PaymentLedgerPostingLineContract(
                x.AccountCode,
                x.AccountName,
                x.DebitAmount,
                x.CreditAmount,
                x.Description)).ToList());

        return new PostPaymentJournalCommand(ledgerId, contract, postedAtUtc);
    }
}
