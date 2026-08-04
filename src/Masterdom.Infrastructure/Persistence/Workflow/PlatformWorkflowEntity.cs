using System;

namespace Masterdom.Infrastructure.Persistence.Workflow;

/// <summary>
/// Persistence model for workflow definitions.
/// </summary>
public sealed class PlatformWorkflowEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ScopeKind { get; set; }

    public string? ScopeIdentifier { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }
}
