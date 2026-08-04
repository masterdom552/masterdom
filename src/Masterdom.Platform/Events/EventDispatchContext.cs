using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents execution context for one dispatch operation.
/// </summary>
public sealed class EventDispatchContext
{
    public required EventEnvelope Envelope { get; init; }

    public required EventDispatchPolicy Policy { get; init; }

    public required DateTime StartedAtUtc { get; init; }
}
