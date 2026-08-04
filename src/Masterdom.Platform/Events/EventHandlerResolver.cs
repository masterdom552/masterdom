using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// Resolves handlers from the event registry.
/// </summary>
public sealed class EventHandlerResolver : IEventHandlerResolver
{
    private readonly IEventRegistry _registry;

    public EventHandlerResolver(IEventRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public IReadOnlyList<IEventHandler> Resolve(EventEnvelope envelope, EventDispatchPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(policy);

        var handlers = _registry.GetHandlers(envelope.EventType);

        return policy.Ordering == EventDispatchOrdering.ExplicitOrder
            ? handlers
                .OrderBy(x => x.Descriptor.ExplicitOrder)
                .ThenBy(x => x.Descriptor.HandlerId, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : handlers;
    }
}
