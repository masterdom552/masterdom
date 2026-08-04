namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents an optional compensation hook descriptor.
/// </summary>
public sealed class WorkflowCompensationHook
{
    public WorkflowCompensationHook(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new WorkflowValidationException("Compensation operation is required.");
        }

        Operation = operation.Trim();
    }

    public string Operation { get; }
}
