namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class Journal
{
    private readonly IReadOnlyList<JournalEntry> _entries;

    private Journal(string journalNumber, string description, IReadOnlyList<JournalEntry> entries)
    {
        JournalNumber = journalNumber;
        Description = description;
        _entries = entries;
    }

    public string JournalNumber { get; private set; }

    public string Description { get; private set; }

    public IReadOnlyList<JournalEntry> Entries => _entries;

    public static Journal Create(string journalNumber, string description, IEnumerable<JournalEntry> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journalNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(entries);

        var list = entries.ToList();
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Journal must contain at least one entry.");
        }

        var debitTotal = list.Sum(x => x.DebitAmount.Value);
        var creditTotal = list.Sum(x => x.CreditAmount.Value);

        if (debitTotal != creditTotal)
        {
            throw new InvalidOperationException("Debit total must equal credit total.");
        }

        return new Journal(journalNumber.Trim(), description.Trim(), list.AsReadOnly());
    }

    public decimal GetDebitTotal()
    {
        return _entries.Sum(x => x.DebitAmount.Value);
    }

    public decimal GetCreditTotal()
    {
        return _entries.Sum(x => x.CreditAmount.Value);
    }
}
