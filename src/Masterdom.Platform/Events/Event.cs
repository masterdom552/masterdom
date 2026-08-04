using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// Base immutable event implementation.
/// </summary>
public abstract class Event : IEvent
{
    protected Event(
        EventId eventId,
        EventVersion eventVersion,
        EventCategory category,
        EventType eventType,
        DateTime occurredAtUtc,
        EventPayload payload,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new EventValidationException("Event occurrence timestamp must be UTC.");
        }

        EventId = eventId;
        EventVersion = eventVersion;
        Category = category;
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        OccurredAtUtc = occurredAtUtc;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Headers = (headers ?? new Dictionary<string, string>())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        Metadata = (metadata ?? new Dictionary<string, string>())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    public EventId EventId { get; }

    public EventVersion EventVersion { get; }

    public EventCategory Category { get; }

    public EventType EventType { get; }

    public DateTime OccurredAtUtc { get; }

    public EventPayload Payload { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
