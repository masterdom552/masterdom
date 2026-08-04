using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Commands;

public sealed record PostBillingJournalCommand(
    LedgerId LedgerId,
    BillingLedgerPostingContract Contract,
    DateTime PostedAtUtc);

public static class PostBillingJournalCommandFactory
{
    public static PostBillingJournalCommand Create(
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

        var contract = new BillingLedgerPostingContract(
            postingReference,
            journalNumber,
            postingDate,
            description,
            batchReference,
            lines.Select(x => new LedgerPostingLineContract(
                x.AccountCode,
                x.AccountName,
                x.DebitAmount,
                x.CreditAmount,
                x.Description)).ToList());

        return new PostBillingJournalCommand(ledgerId, contract, postedAtUtc);
    }
}
