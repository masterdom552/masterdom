namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a workflow version number.
/// </summary>
public readonly struct WorkflowVersion
{
    public WorkflowVersion(int value)
    {
        if (value <= 0)
        {
            throw new WorkflowValidationException("Workflow version must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
