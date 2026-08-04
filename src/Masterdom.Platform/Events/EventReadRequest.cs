using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents query criteria when reading stored events.
/// </summary>
public sealed class EventReadRequest
{
    public EventType? EventType { get; init; }

    public string? AggregateId { get; init; }

    public string? ModuleId { get; init; }

    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}
