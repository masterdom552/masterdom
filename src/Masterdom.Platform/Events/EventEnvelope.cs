using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents the canonical event envelope used by the dispatch pipeline.
/// </summary>
public sealed class EventEnvelope
{
    public EventEnvelope(
        IEvent @event,
        EventContext context,
        IReadOnlyDictionary<string, string>? headers = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.Validate();

        EventId = @event.EventId;
        EventVersion = @event.EventVersion;
        OccurredAtUtc = @event.OccurredAtUtc;
        CorrelationId = context.CorrelationId;
        CausationId = context.CausationId;
        TenantId = context.TenantId;
        ModuleId = context.ModuleId.Trim();
        AggregateId = context.AggregateId;
        AggregateType = context.AggregateType;
        EventType = @event.EventType;
        Payload = @event.Payload;

        Headers = Merge(
            @event.Headers,
            headers);

        Metadata = Merge(
            @event.Metadata,
            metadata);
    }

    public IEvent Event { get; }

    public EventId EventId { get; }

    public EventVersion EventVersion { get; }

    public DateTime OccurredAtUtc { get; }

    public string? CorrelationId { get; }

    public string? CausationId { get; }

    public string? TenantId { get; }

    public string ModuleId { get; }

    public string? AggregateId { get; }

    public string? AggregateType { get; }

    public EventType EventType { get; }

    public EventPayload Payload { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public EventContext Context { get; }

    private static IReadOnlyDictionary<string, string> Merge(
        IReadOnlyDictionary<string, string> baseline,
        IReadOnlyDictionary<string, string>? overlay)
    {
        var merged = baseline
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        if (overlay is null)
        {
            return merged;
        }

        foreach (var pair in overlay)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }
}
