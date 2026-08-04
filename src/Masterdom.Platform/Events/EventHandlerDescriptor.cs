using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Describes handler identity, subscription, and ordering metadata.
/// </summary>
public sealed class EventHandlerDescriptor
{
    public required string HandlerId { get; init; }

    public required EventType SubscribedEventType { get; init; }

    public int ExplicitOrder { get; init; }

    public IReadOnlyList<EventType> EmitsEventTypes { get; init; } =
        new List<EventType>();
}
