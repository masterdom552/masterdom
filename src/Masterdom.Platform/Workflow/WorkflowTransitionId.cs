using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents the unique identity of a workflow transition.
/// </summary>
public readonly struct WorkflowTransitionId
{
    public WorkflowTransitionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new WorkflowValidationException("WorkflowTransitionId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
