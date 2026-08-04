using System;
using System.Collections.Generic;
using System.Text.Json;
using DomainEventContract = Masterdom.Core.Common.Events.IDomainEvent;

namespace Masterdom.Platform.Events;

/// <summary>
/// Default adapter from core domain events to platform event envelopes.
/// </summary>
public sealed class DomainEventAdapter : IDomainEventAdapter
{
    public DomainRuntimeEvent Adapt(DomainEventContract domainEvent, EventContext context)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        ArgumentNullException.ThrowIfNull(context);

        context.Validate();

        var eventType = new EventType(domainEvent.GetType().Name);
        var payloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        return new DomainRuntimeEvent(
            new EventId(Guid.NewGuid()),
            new EventVersion(1),
            eventType,
            domainEvent.OccurredOnUtc,
            new EventPayload(payloadJson),
            domainEvent,
            metadata: new Dictionary<string, string>
            {
                ["source"] = "domain"
            });
    }
}
