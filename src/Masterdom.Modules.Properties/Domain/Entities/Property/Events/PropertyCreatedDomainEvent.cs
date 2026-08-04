using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Properties.Domain.Entities.Property.Events;

/// <summary>
/// Domain fact emitted when a property is created.
/// </summary>
public sealed class PropertyCreatedDomainEvent : DomainEvent
{
    public PropertyCreatedDomainEvent(PropertyId propertyId, PropertyCode code)
    {
        ArgumentNullException.ThrowIfNull(propertyId);
        ArgumentNullException.ThrowIfNull(code);

        PropertyId = propertyId;
        Code = code;
    }

    public PropertyId PropertyId { get; }

    public PropertyCode Code { get; }
}
