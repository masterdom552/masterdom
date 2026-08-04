using System;

namespace Masterdom.Platform.Events;

/// <summary>
/// Represents execution output for one handler in the pipeline.
/// </summary>
public sealed class EventHandlerDispatchResult
{
    public required string HandlerId { get; init; }

    public required bool IsSuccess { get; init; }

    public required TimeSpan ExecutionTime { get; init; }

    public string? Message { get; init; }
}
