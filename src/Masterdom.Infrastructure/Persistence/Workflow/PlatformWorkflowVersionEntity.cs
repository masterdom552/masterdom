using System;

namespace Masterdom.Infrastructure.Persistence.Workflow;

/// <summary>
/// Persistence model for workflow version definitions.
/// </summary>
public sealed class PlatformWorkflowVersionEntity
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public int Version { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public DateTime? EffectiveToUtc { get; set; }

    public bool IsDeprecated { get; set; }

    public Guid? ReplacedByVersionId { get; set; }

    public string? Compatibility { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }
}
