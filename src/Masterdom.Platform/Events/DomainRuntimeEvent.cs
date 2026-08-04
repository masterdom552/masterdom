using System;
using System.Collections.Generic;
using DomainEventContract = Masterdom.Core.Common.Events.IDomainEvent;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an adapted domain event wrapped by platform contracts.
/// </summary>
public sealed class DomainRuntimeEvent : Event, IDomainRuntimeEvent
{
    public DomainRuntimeEvent(
        EventId eventId,
        EventVersion eventVersion,
        EventType eventType,
        DateTime occurredAtUtc,
        EventPayload payload,
        DomainEventContract domainEvent,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        : base(
            eventId,
            eventVersion,
            EventCategory.Domain,
            eventType,
            occurredAtUtc,
            payload,
            headers,
            metadata)
    {
        DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }

    public DomainEventContract DomainEvent { get; }
}
