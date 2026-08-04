using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents the unique identity of a workflow step.
/// </summary>
public readonly struct WorkflowStepId
{
    public WorkflowStepId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new WorkflowValidationException("WorkflowStepId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
