using System;
using System.Collections.Generic;
using Masterdom.Core.Common.Entities;
using Masterdom.Core.Common.Events;
using Masterdom.Platform.Events;

namespace Masterdom.Platform.Tests.Events;

public sealed class DomainEventPublisherTests
{
    [Fact]
    public void Publish_ShouldAdaptAggregateDomainEventsAndClearAfterPublish()
    {
        var aggregate = TestAggregate.Create();
        aggregate.MarkSomethingHappened();
        aggregate.MarkSomethingHappened();

        var registry = new EventRegistry();
        registry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType(nameof(TestDomainEvent)),
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
            aggregate,
            new EventContext
            {
                ModuleId = "domain-test",
                CorrelationId = "corr-domain",
                CausationId = "cause-domain",
                AggregateId = aggregate.Id.ToString("N"),
                AggregateType = nameof(TestAggregate),
                OccurredAtUtc = DateTime.UtcNow
            });

        Assert.Equal(2, result.PublishedCount);
        Assert.Empty(aggregate.DomainEvents);
        Assert.All(result.PublishedEvents, published =>
        {
            Assert.Equal("corr-domain", published.Envelope.CorrelationId);
            Assert.Equal(nameof(TestDomainEvent), published.Envelope.EventType.Value);
        });
    }

    private sealed class TestAggregate : AggregateRoot
    {
        private TestAggregate(Guid id)
            : base(id)
        {
        }

        public static TestAggregate Create()
        {
            return new TestAggregate(Guid.NewGuid());
        }

        public void MarkSomethingHappened()
        {
            Raise(new TestDomainEvent("something-happened"));
        }
    }

    private sealed class TestDomainEvent : DomainEvent
    {
        public TestDomainEvent(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
