using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class MeterLocationReference : ValueObject
{
    private MeterLocationReference(Guid propertyId, Guid unitId)
    {
        PropertyId = propertyId;
        UnitId = unitId;
    }

    public Guid PropertyId { get; }

    public Guid UnitId { get; }

    public static MeterLocationReference Create(Guid propertyId, Guid unitId)
    {
        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property id cannot be empty.", nameof(propertyId));
        }

        if (unitId == Guid.Empty)
        {
            throw new ArgumentException("Unit id cannot be empty.", nameof(unitId));
        }

        return new MeterLocationReference(propertyId, unitId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PropertyId;
        yield return UnitId;
    }
}
