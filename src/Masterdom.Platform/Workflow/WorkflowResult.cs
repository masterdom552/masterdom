namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents the output of workflow execution.
/// </summary>
public sealed class WorkflowResult
{
    public required WorkflowState State { get; init; }

    public bool IsTerminal => State.Status is WorkflowExecutionStatus.Completed or WorkflowExecutionStatus.Failed or WorkflowExecutionStatus.Cancelled or WorkflowExecutionStatus.TimedOut;
}
