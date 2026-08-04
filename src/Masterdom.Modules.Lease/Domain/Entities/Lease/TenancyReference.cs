using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents tenancy identity reference for lease ownership boundary.
/// </summary>
public sealed class TenancyReference : ValueObject
{
    private TenancyReference(Guid tenancyId)
    {
        TenancyId = tenancyId;
    }

    public Guid TenancyId { get; }

    public static TenancyReference Create(Guid tenancyId)
    {
        if (tenancyId == Guid.Empty)
        {
            throw new ArgumentException("Tenancy reference cannot be empty.", nameof(tenancyId));
        }

        return new TenancyReference(tenancyId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TenancyId;
    }
}
