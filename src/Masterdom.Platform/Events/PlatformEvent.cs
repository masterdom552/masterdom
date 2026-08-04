using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents a concrete platform event instance.
/// </summary>
public sealed class PlatformEvent : Event, IPlatformEvent
{
    public PlatformEvent(
        EventId eventId,
        EventVersion eventVersion,
        EventType eventType,
        DateTime occurredAtUtc,
        EventPayload payload,
        EventCategory category = EventCategory.Platform,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        : base(
            eventId,
            eventVersion,
            category,
            eventType,
            occurredAtUtc,
            payload,
            headers,
            metadata)
    {
    }
}
