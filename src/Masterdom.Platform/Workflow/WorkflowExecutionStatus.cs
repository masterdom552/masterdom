namespace Masterdom.Platform.Workflow;

/// <summary>
/// Defines workflow execution statuses.
/// </summary>
public enum WorkflowExecutionStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    TimedOut = 4
}
