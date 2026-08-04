using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents the unique identity of a workflow.
/// </summary>
public readonly struct WorkflowId
{
    public WorkflowId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new WorkflowValidationException("WorkflowId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
