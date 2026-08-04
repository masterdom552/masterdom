namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class LedgerVersion
{
    private LedgerVersion(int versionNumber, string changeReason, DateTime createdAtUtc)
    {
        VersionNumber = versionNumber;
        ChangeReason = changeReason;
        CreatedAtUtc = createdAtUtc;
    }

    public int VersionNumber { get; private set; }

    public string ChangeReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static LedgerVersion Create(int versionNumber, string changeReason, DateTime createdAtUtc)
    {
        if (versionNumber <= 0)
        {
            throw new InvalidOperationException("Ledger version number must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger version timestamp must be UTC.");
        }

        return new LedgerVersion(versionNumber, changeReason.Trim(), createdAtUtc);
    }
}
