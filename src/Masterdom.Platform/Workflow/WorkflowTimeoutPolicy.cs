using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Defines timeout policy for workflow steps.
/// </summary>
public readonly struct WorkflowTimeoutPolicy
{
    public WorkflowTimeoutPolicy(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new WorkflowValidationException("Workflow timeout cannot be negative.");
        }

        Timeout = timeout;
    }

    public TimeSpan Timeout { get; }

    public static WorkflowTimeoutPolicy None()
    {
        return new WorkflowTimeoutPolicy(TimeSpan.Zero);
    }
}
