using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a workflow definition root.
/// </summary>
public sealed class WorkflowDefinition
{
    public WorkflowDefinition(
        WorkflowId id,
        WorkflowKey key,
        string name,
        string? description,
        WorkflowScope scope,
        string changedBy,
        DateTime changedAtUtc)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new WorkflowValidationException("Workflow name is required.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new WorkflowValidationException("ChangedBy is required for workflows.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new WorkflowValidationException("ChangedAtUtc must be UTC for workflows.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ChangedBy = changedBy.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public WorkflowId Id { get; }

    public WorkflowKey Key { get; }

    public string Name { get; }

    public string? Description { get; }

    public WorkflowScope Scope { get; }

    public string ChangedBy { get; }

    public DateTime ChangedAtUtc { get; }
}
