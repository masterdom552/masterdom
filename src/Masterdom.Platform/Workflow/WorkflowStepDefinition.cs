using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a workflow step within a workflow version.
/// </summary>
public sealed class WorkflowStepDefinition
{
    public WorkflowStepDefinition(
        WorkflowStepId id,
        WorkflowVersionId workflowVersionId,
        string key,
        string name,
        WorkflowStepKind kind,
        bool isStart,
        bool isTerminal,
        WorkflowRetryPolicy retryPolicy,
        WorkflowTimeoutPolicy timeoutPolicy,
        WorkflowCompensationHook? compensationHook)
    {
        Id = id;
        WorkflowVersionId = workflowVersionId;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new WorkflowValidationException("Workflow step key is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new WorkflowValidationException("Workflow step name is required.");
        }

        if (isStart && isTerminal && kind == WorkflowStepKind.ManualApproval)
        {
            throw new WorkflowValidationException("Manual approval step cannot be both start and terminal.");
        }

        Key = key.Trim();
        Name = name.Trim();
        Kind = kind;
        IsStart = isStart;
        IsTerminal = isTerminal;
        RetryPolicy = retryPolicy;
        TimeoutPolicy = timeoutPolicy;
        CompensationHook = compensationHook;
    }

    public WorkflowStepId Id { get; }

    public WorkflowVersionId WorkflowVersionId { get; }

    public string Key { get; }

    public string Name { get; }

    public WorkflowStepKind Kind { get; }

    public bool IsStart { get; }

    public bool IsTerminal { get; }

    public WorkflowRetryPolicy RetryPolicy { get; }

    public WorkflowTimeoutPolicy TimeoutPolicy { get; }

    public WorkflowCompensationHook? CompensationHook { get; }
}
