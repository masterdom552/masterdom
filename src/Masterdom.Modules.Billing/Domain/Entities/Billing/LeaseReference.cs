using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents lease reference for billing ownership boundary.
/// </summary>
public sealed class LeaseReference : ValueObject
{
    private LeaseReference(Guid leaseId)
    {
        LeaseId = leaseId;
    }

    public Guid LeaseId { get; }

    public static LeaseReference Create(Guid leaseId)
    {
        if (leaseId == Guid.Empty)
        {
            throw new ArgumentException("Lease reference cannot be empty.", nameof(leaseId));
        }

        return new LeaseReference(leaseId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LeaseId;
    }
}
