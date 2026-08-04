using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents an immutable event contract.
/// </summary>
public interface IEvent
{
    EventId EventId { get; }

    EventVersion EventVersion { get; }

    EventCategory Category { get; }

    EventType EventType { get; }

    DateTime OccurredAtUtc { get; }

    EventPayload Payload { get; }

    IReadOnlyDictionary<string, string> Headers { get; }

    IReadOnlyDictionary<string, string> Metadata { get; }
}
