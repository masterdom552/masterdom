using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a workflow execution history event.
/// </summary>
public sealed class WorkflowExecutionEvent
{
    public required DateTime TimestampUtc { get; init; }

    public required string EventType { get; init; }

    public WorkflowStepId? StepId { get; init; }

    public required string Message { get; init; }
}
