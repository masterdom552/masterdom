namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class JournalEntry
{
    private JournalEntry(Guid entryId, AccountReference accountReference, MoneyAmount debitAmount, MoneyAmount creditAmount, string description)
    {
        EntryId = entryId;
        AccountReference = accountReference;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
        Description = description;
    }

    public Guid EntryId { get; private set; }

    public AccountReference AccountReference { get; private set; }

    public MoneyAmount DebitAmount { get; private set; }

    public MoneyAmount CreditAmount { get; private set; }

    public string Description { get; private set; }

    public static JournalEntry Create(AccountReference accountReference, MoneyAmount debitAmount, MoneyAmount creditAmount, string description)
    {
        ArgumentNullException.ThrowIfNull(accountReference);
        ArgumentNullException.ThrowIfNull(debitAmount);
        ArgumentNullException.ThrowIfNull(creditAmount);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var hasDebit = debitAmount.Value > 0m;
        var hasCredit = creditAmount.Value > 0m;

        if (hasDebit == hasCredit)
        {
            throw new InvalidOperationException("Journal entry must contain either a debit amount or a credit amount.");
        }

        return new JournalEntry(Guid.CreateVersion7(), accountReference, debitAmount, creditAmount, description.Trim());
    }
}
