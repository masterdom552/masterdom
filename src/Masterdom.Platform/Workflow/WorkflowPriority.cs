namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents workflow transition priority.
/// </summary>
public readonly struct WorkflowPriority
{
    public WorkflowPriority(int value)
    {
        if (value < 1 || value > 1000)
        {
            throw new WorkflowValidationException("Workflow priority must be between 1 and 1000.");
        }

        Value = value;
    }

    public int Value { get; }
}
