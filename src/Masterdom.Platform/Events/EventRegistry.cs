using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// Cached in-memory event and handler registry.
/// </summary>
public sealed class EventRegistry : IEventRegistry
{
    private readonly Dictionary<string, EventDescriptor> _eventsByType =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, List<HandlerRegistration>> _handlersByType =
        new(StringComparer.OrdinalIgnoreCase);

    private long _registrationSequence;

    public void RegisterEvent(EventDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var key = descriptor.EventType.Value;

        if (_eventsByType.ContainsKey(key))
        {
            throw new EventValidationException(
                $"Duplicate event descriptor for event type '{key}'.");
        }

        _eventsByType[key] = descriptor;
    }

    public void RegisterEvents(IReadOnlyList<EventDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        foreach (var descriptor in descriptors)
        {
            RegisterEvent(descriptor);
        }
    }

    public void RegisterHandler(IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var descriptor = handler.Descriptor ??
            throw new EventValidationException("Handler descriptor is required.");

        if (string.IsNullOrWhiteSpace(descriptor.HandlerId))
        {
            throw new EventValidationException("HandlerId is required.");
        }

        var eventTypeKey = descriptor.SubscribedEventType.Value;
        if (!_handlersByType.TryGetValue(eventTypeKey, out var handlers))
        {
            handlers = new List<HandlerRegistration>();
            _handlersByType[eventTypeKey] = handlers;
        }

        if (handlers.Any(x => string.Equals(
            x.Handler.Descriptor.HandlerId,
            descriptor.HandlerId,
            StringComparison.OrdinalIgnoreCase)))
        {
            throw new EventValidationException(
                $"Duplicate handler '{descriptor.HandlerId}' for event type '{eventTypeKey}'.");
        }

        handlers.Add(new HandlerRegistration(
            handler,
            ++_registrationSequence));
    }

    public void RegisterSubscriber(IEventSubscriber subscriber)
    {
        RegisterHandler(subscriber);
    }

    public IEventCatalog GetCatalog()
    {
        var handlers = _handlersByType
            .SelectMany(pair => pair.Value)
            .Select(registration => registration.Handler.Descriptor)
            .ToList();

        return new EventCatalog(_eventsByType.Values, handlers);
    }

    public IReadOnlyList<IEventHandler> GetHandlers(EventType eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (!_handlersByType.TryGetValue(eventType.Value, out var registrations))
        {
            return Array.Empty<IEventHandler>();
        }

        return registrations
            .OrderBy(x => x.Sequence)
            .Select(x => x.Handler)
            .ToList();
    }

    public void Validate()
    {
        ValidateSubscriptions();
        ValidateRequiredHandlers();
        ValidateCircularDispatch();
    }

    private void ValidateSubscriptions()
    {
        foreach (var subscription in _handlersByType.Keys)
        {
            if (_eventsByType.ContainsKey(subscription))
            {
                continue;
            }

            throw new EventValidationException(
                $"Invalid subscription: event type '{subscription}' is not registered.");
        }
    }

    private void ValidateRequiredHandlers()
    {
        foreach (var descriptor in _eventsByType.Values)
        {
            if (!descriptor.RequiresHandler)
            {
                continue;
            }

            if (_handlersByType.TryGetValue(descriptor.EventType.Value, out var handlers) &&
                handlers.Count > 0)
            {
                continue;
            }

            throw new EventValidationException(
                $"Missing handlers for required event type '{descriptor.EventType.Value}'.");
        }
    }

    private void ValidateCircularDispatch()
    {
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var eventType in _eventsByType.Keys)
        {
            graph[eventType] = new List<string>();
        }

        foreach (var registrations in _handlersByType.Values)
        {
            foreach (var registration in registrations)
            {
                var from = registration.Handler.Descriptor.SubscribedEventType.Value;
                foreach (var emitted in registration.Handler.Descriptor.EmitsEventTypes)
                {
                    if (!graph.ContainsKey(from))
                    {
                        graph[from] = new List<string>();
                    }

                    graph[from].Add(emitted.Value);
                }
            }
        }

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
        {
            if (HasCycle(node, graph, visiting, visited))
            {
                throw new EventValidationException(
                    $"Circular dispatch dependency detected starting from event type '{node}'.");
            }
        }
    }

    private static bool HasCycle(
        string node,
        IReadOnlyDictionary<string, List<string>> graph,
        HashSet<string> visiting,
        HashSet<string> visited)
    {
        if (visited.Contains(node))
        {
            return false;
        }

        if (!visiting.Add(node))
        {
            return true;
        }

        if (graph.TryGetValue(node, out var outgoing))
        {
            foreach (var next in outgoing)
            {
                if (HasCycle(next, graph, visiting, visited))
                {
                    return true;
                }
            }
        }

        visiting.Remove(node);
        visited.Add(node);
        return false;
    }

    private sealed record HandlerRegistration(IEventHandler Handler, long Sequence);
}
