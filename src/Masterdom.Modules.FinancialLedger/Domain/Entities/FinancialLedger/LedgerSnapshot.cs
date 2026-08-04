namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class LedgerSnapshot
{
    private LedgerSnapshot(Guid snapshotId, int versionNumber, int transactionCount, int accountCount, MoneyAmount totalDebits, MoneyAmount totalCredits, DateTime capturedAtUtc)
    {
        SnapshotId = snapshotId;
        VersionNumber = versionNumber;
        TransactionCount = transactionCount;
        AccountCount = accountCount;
        TotalDebits = totalDebits;
        TotalCredits = totalCredits;
        CapturedAtUtc = capturedAtUtc;
    }

    public Guid SnapshotId { get; private set; }

    public int VersionNumber { get; private set; }

    public int TransactionCount { get; private set; }

    public int AccountCount { get; private set; }

    public MoneyAmount TotalDebits { get; private set; }

    public MoneyAmount TotalCredits { get; private set; }

    public DateTime CapturedAtUtc { get; private set; }

    public static LedgerSnapshot Capture(int versionNumber, int transactionCount, int accountCount, MoneyAmount totalDebits, MoneyAmount totalCredits, DateTime capturedAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Ledger snapshot version number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(totalDebits);
        ArgumentNullException.ThrowIfNull(totalCredits);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger snapshot timestamp must be UTC.");
        }

        return new LedgerSnapshot(Guid.CreateVersion7(), versionNumber, transactionCount, accountCount, totalDebits, totalCredits, capturedAtUtc);
    }
}
