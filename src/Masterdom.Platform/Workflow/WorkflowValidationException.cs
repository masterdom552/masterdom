using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents workflow framework validation failures.
/// </summary>
public sealed class WorkflowValidationException : Exception
{
    public WorkflowValidationException(string message)
        : base(message)
    {
    }
}
