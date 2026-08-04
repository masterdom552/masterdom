using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents aggregate output for one dispatch operation.
/// </summary>
public sealed class EventDispatchResult
{
    public required EventId EventId { get; init; }

    public required EventType EventType { get; init; }

    public required TimeSpan ExecutionTime { get; init; }

    public required int HandlerCount { get; init; }

    public required int SuccessfulHandlers { get; init; }

    public required int FailedHandlers { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }

    public required IReadOnlyList<EventDispatchDiagnostic> Diagnostics { get; init; }

    public required IReadOnlyList<EventHandlerDispatchResult> HandlerResults { get; init; }
}
