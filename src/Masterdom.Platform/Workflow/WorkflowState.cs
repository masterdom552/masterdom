using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents persisted workflow execution state.
/// </summary>
public sealed class WorkflowState
{
    public required Guid InstanceId { get; init; }

    public required WorkflowId WorkflowId { get; init; }

    public required WorkflowVersionId WorkflowVersionId { get; init; }

    public WorkflowStepId? CurrentStepId { get; set; }

    public required WorkflowExecutionStatus Status { get; set; }

    public required List<WorkflowStepId> CompletedSteps { get; init; }

    public required List<WorkflowStepId> PendingSteps { get; init; }

    public required List<WorkflowExecutionEvent> History { get; init; }

    public string? Error { get; set; }

    public required DateTime StartedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; set; }
}
