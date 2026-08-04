using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents the unique identity of a workflow version.
/// </summary>
public readonly struct WorkflowVersionId
{
    public WorkflowVersionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new WorkflowValidationException("WorkflowVersionId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
