using Masterdom.Core.Primitives;

namespace Masterdom.Core.Financial.ValueObjects;

public sealed class LedgerReference : ValueObject
{
    public string LedgerId { get; }

    public string EntryId { get; }

    private LedgerReference(string ledgerId, string entryId)
    {
        LedgerId = ledgerId;
        EntryId = entryId;
    }

    public static LedgerReference Create(string ledgerId, string entryId)
    {
        if (string.IsNullOrWhiteSpace(ledgerId))
            throw new ArgumentException("Ledger id is required.", nameof(ledgerId));

        if (string.IsNullOrWhiteSpace(entryId))
            throw new ArgumentException("Entry id is required.", nameof(entryId));

        return new LedgerReference(ledgerId.Trim(), entryId.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LedgerId;
        yield return EntryId;
    }
}
