using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// In-memory event repository implementation.
/// </summary>
public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<EventEnvelope> _events = new();
    private readonly HashSet<Guid> _eventIds = new();

    public void Save(EventEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (!_eventIds.Add(envelope.EventId.Value))
        {
            throw new EventValidationException(
                $"Duplicate event identifier detected: '{envelope.EventId.Value}'.");
        }

        _events.Add(envelope);
    }

    public IReadOnlyList<EventEnvelope> Read(EventReadRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _events
            .Where(e => request.EventType is null || e.EventType.Equals(request.EventType))
            .Where(e => string.IsNullOrWhiteSpace(request.AggregateId) ||
                        string.Equals(e.AggregateId, request.AggregateId, StringComparison.OrdinalIgnoreCase))
            .Where(e => string.IsNullOrWhiteSpace(request.ModuleId) ||
                        string.Equals(e.ModuleId, request.ModuleId, StringComparison.OrdinalIgnoreCase))
            .Where(e => !request.FromUtc.HasValue || e.OccurredAtUtc >= request.FromUtc.Value)
            .Where(e => !request.ToUtc.HasValue || e.OccurredAtUtc <= request.ToUtc.Value)
            .OrderBy(e => e.OccurredAtUtc)
            .ToList();
    }
}
