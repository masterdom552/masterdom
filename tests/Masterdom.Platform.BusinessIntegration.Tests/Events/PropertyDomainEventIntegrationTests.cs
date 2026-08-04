using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Entities.Property.Events;
using Masterdom.Platform.Events;

namespace Masterdom.Platform.Tests.Events;

public sealed class PropertyDomainEventIntegrationTests
{
    [Fact]
    public void Publish_ShouldAdaptPropertyAggregateDomainEvents()
    {
        var property = Property.Create(
            new PropertyCode("EVT-PROP-01"),
            new PropertyName("Property Event Building"),
            PropertyType.Commercial);

        property.CreateUnit(
            new UnitCode("UNIT-01"),
            "Unit 01",
            UnitType.Office);

        var registry = new EventRegistry();
        registry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType(nameof(PropertyCreatedDomainEvent)),
            Category = EventCategory.Domain,
            Version = new EventVersion(1)
        });
        registry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType(nameof(UnitCreatedDomainEvent)),
            Category = EventCategory.Domain,
            Version = new EventVersion(1)
        });

        var publisher = new EventPublisher(
            new EventStore(new InMemoryEventRepository()),
            new EventDispatcher(new EventHandlerResolver(registry)));

        var domainPublisher = new DomainEventPublisher(
            new DomainEventAdapter(),
            publisher);

        var result = domainPublisher.Publish(
            property,
            new EventContext
            {
                ModuleId = "properties",
                CorrelationId = "corr-properties",
                CausationId = "cause-properties",
                AggregateId = property.Id.ToString(),
                AggregateType = nameof(Property),
                OccurredAtUtc = DateTime.UtcNow
            });

        Assert.Equal(2, result.PublishedCount);
        Assert.Empty(property.DomainEvents);
        Assert.Contains(result.PublishedEvents, x => x.Envelope.EventType.Value == nameof(PropertyCreatedDomainEvent));
        Assert.Contains(result.PublishedEvents, x => x.Envelope.EventType.Value == nameof(UnitCreatedDomainEvent));
    }
}
