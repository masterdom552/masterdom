using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Properties.Domain.Entities.Property.Events;

/// <summary>
/// Domain fact emitted when a property display name changes.
/// </summary>
public sealed class PropertyRenamedDomainEvent : DomainEvent
{
    public PropertyRenamedDomainEvent(PropertyId propertyId, PropertyName name)
    {
        ArgumentNullException.ThrowIfNull(propertyId);
        ArgumentNullException.ThrowIfNull(name);

        PropertyId = propertyId;
        Name = name;
    }

    public PropertyId PropertyId { get; }

    public PropertyName Name { get; }
}
