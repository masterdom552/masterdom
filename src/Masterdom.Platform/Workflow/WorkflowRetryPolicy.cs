using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Defines retry policy for automatic workflow steps.
/// </summary>
public readonly struct WorkflowRetryPolicy
{
    public WorkflowRetryPolicy(int maxAttempts, TimeSpan retryDelay)
    {
        if (maxAttempts < 0 || maxAttempts > 100)
        {
            throw new WorkflowValidationException("Max retry attempts must be between 0 and 100.");
        }

        if (retryDelay < TimeSpan.Zero)
        {
            throw new WorkflowValidationException("Retry delay cannot be negative.");
        }

        MaxAttempts = maxAttempts;
        RetryDelay = retryDelay;
    }

    public int MaxAttempts { get; }

    public TimeSpan RetryDelay { get; }

    public static WorkflowRetryPolicy None()
    {
        return new WorkflowRetryPolicy(0, TimeSpan.Zero);
    }
}
