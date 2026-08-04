using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Properties.Domain.Entities.Property.Events;

/// <summary>
/// Domain fact emitted when a unit is created for a property.
/// </summary>
public sealed class UnitCreatedDomainEvent : DomainEvent
{
    public UnitCreatedDomainEvent(
        PropertyId propertyId,
        UnitId unitId,
        UnitCode unitCode)
    {
        ArgumentNullException.ThrowIfNull(propertyId);
        ArgumentNullException.ThrowIfNull(unitId);
        ArgumentNullException.ThrowIfNull(unitCode);

        PropertyId = propertyId;
        UnitId = unitId;
        UnitCode = unitCode;
    }

    public PropertyId PropertyId { get; }

    public UnitId UnitId { get; }

    public UnitCode UnitCode { get; }
}
