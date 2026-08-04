using System;
using System.Collections.Generic;
using Masterdom.Core.Common.Interfaces;

namespace Masterdom.Platform.Events;

/// <summary>
/// Publishes aggregate domain events through an event publisher.
/// </summary>
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IDomainEventAdapter _adapter;
    private readonly IEventPublisher _publisher;

    public DomainEventPublisher(IDomainEventAdapter adapter, IEventPublisher publisher)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public DomainEventPublishResult Publish(IHasDomainEvents aggregate, EventContext context, EventDispatchPolicy? policy = null, bool clearAfterPublish = true)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(context);

        var published = new List<EventPublishResult>();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            var runtimeEvent = _adapter.Adapt(domainEvent, context);
            var envelope = new EventEnvelope(runtimeEvent, context);
            published.Add(_publisher.Publish(envelope, policy));
        }

        if (clearAfterPublish)
        {
            aggregate.ClearDomainEvents();
        }

        return new DomainEventPublishResult
        {
            PublishedCount = published.Count,
            PublishedEvents = published
        };
    }
}
