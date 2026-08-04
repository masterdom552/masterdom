using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Properties.Domain.Entities.Property.Events;

/// <summary>
/// Domain fact emitted when a unit is removed from a property.
/// </summary>
public sealed class UnitRemovedDomainEvent : DomainEvent
{
    public UnitRemovedDomainEvent(PropertyId propertyId, UnitId unitId)
    {
        ArgumentNullException.ThrowIfNull(propertyId);
        ArgumentNullException.ThrowIfNull(unitId);

        PropertyId = propertyId;
        UnitId = unitId;
    }

    public PropertyId PropertyId { get; }

    public UnitId UnitId { get; }
}
