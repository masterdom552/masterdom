using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Properties.Domain.Entities.Property.Events;

/// <summary>
/// Domain fact emitted when property lifecycle status changes.
/// </summary>
public sealed class PropertyStatusChangedDomainEvent : DomainEvent
{
    public PropertyStatusChangedDomainEvent(PropertyId propertyId, PropertyStatus status)
    {
        ArgumentNullException.ThrowIfNull(propertyId);

        PropertyId = propertyId;
        Status = status;
    }

    public PropertyId PropertyId { get; }

    public PropertyStatus Status { get; }
}
