using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a versioned workflow definition.
/// </summary>
public sealed class WorkflowVersionDefinition
{
    public WorkflowVersionDefinition(
        WorkflowVersionId id,
        WorkflowId workflowId,
        WorkflowVersion version,
        WorkflowEffectivePeriod period,
        bool isDeprecated,
        WorkflowVersionId? replacedBy,
        string? compatibility,
        string changedBy,
        DateTime changedAtUtc)
    {
        Id = id;
        WorkflowId = workflowId;
        Version = version;
        Period = period;

        if (isDeprecated && !replacedBy.HasValue)
        {
            throw new WorkflowValidationException("Deprecated workflow versions must declare replacement.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new WorkflowValidationException("ChangedBy is required for workflow versions.");
        }

        if (changedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new WorkflowValidationException("ChangedAtUtc must be UTC for workflow versions.");
        }

        IsDeprecated = isDeprecated;
        ReplacedBy = replacedBy;
        Compatibility = string.IsNullOrWhiteSpace(compatibility) ? null : compatibility.Trim();
        ChangedBy = changedBy.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public WorkflowVersionId Id { get; }

    public WorkflowId WorkflowId { get; }

    public WorkflowVersion Version { get; }

    public WorkflowEffectivePeriod Period { get; }

    public bool IsDeprecated { get; }

    public WorkflowVersionId? ReplacedBy { get; }

    public string? Compatibility { get; }

    public string ChangedBy { get; }

    public DateTime ChangedAtUtc { get; }
}
