using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// Immutable event catalog snapshot.
/// </summary>
public sealed class EventCatalog : IEventCatalog
{
    public EventCatalog(
        IEnumerable<EventDescriptor> events,
        IEnumerable<EventHandlerDescriptor> handlers)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(handlers);

        Events = events.ToList();
        Handlers = handlers.ToList();
    }

    public IReadOnlyList<EventDescriptor> Events { get; }

    public IReadOnlyList<EventHandlerDescriptor> Handlers { get; }
}
