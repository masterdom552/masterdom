using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents persisted versioned snapshot record.
/// </summary>
public sealed class BillingVersion : ValueObject
{
    private BillingVersion(BillSnapshot snapshot, DateTimeOffset createdAt)
    {
        Snapshot = snapshot;
        CreatedAt = createdAt;
    }

    public BillSnapshot Snapshot { get; }

    public DateTimeOffset CreatedAt { get; }

    public static BillingVersion Create(BillSnapshot snapshot, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new BillingVersion(snapshot, createdAt);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Snapshot;
        yield return CreatedAt;
    }
}
