namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class LedgerAccount
{
    private LedgerAccount(Guid accountId, AccountReference accountReference, string category, DateTime openedAtUtc)
    {
        AccountId = accountId;
        AccountReference = accountReference;
        Category = category;
        OpenedAtUtc = openedAtUtc;
    }

    public Guid AccountId { get; private set; }

    public AccountReference AccountReference { get; private set; }

    public string Category { get; private set; }

    public DateTime OpenedAtUtc { get; private set; }

    public static LedgerAccount Open(AccountReference accountReference, string category, DateTime openedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(accountReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        if (openedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger account opening timestamp must be UTC.");
        }

        return new LedgerAccount(Guid.CreateVersion7(), accountReference, category.Trim(), openedAtUtc);
    }
}
