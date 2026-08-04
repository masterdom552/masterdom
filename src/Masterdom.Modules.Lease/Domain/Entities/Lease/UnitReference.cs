using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents unit identity reference for lease ownership boundary.
/// </summary>
public sealed class UnitReference : ValueObject
{
    private UnitReference(Guid unitId)
    {
        UnitId = unitId;
    }

    public Guid UnitId { get; }

    public static UnitReference Create(Guid unitId)
    {
        if (unitId == Guid.Empty)
        {
            throw new ArgumentException("Unit reference cannot be empty.", nameof(unitId));
        }

        return new UnitReference(unitId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UnitId;
    }
}
