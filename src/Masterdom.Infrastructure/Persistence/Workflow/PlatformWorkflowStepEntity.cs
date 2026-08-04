using System;

namespace Masterdom.Infrastructure.Persistence.Workflow;

/// <summary>
/// Persistence model for workflow steps.
/// </summary>
public sealed class PlatformWorkflowStepEntity
{
    public Guid Id { get; set; }

    public Guid WorkflowVersionId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Kind { get; set; }

    public bool IsStart { get; set; }

    public bool IsTerminal { get; set; }

    public int RetryMaxAttempts { get; set; }

    public int RetryDelayMilliseconds { get; set; }

    public int TimeoutMilliseconds { get; set; }

    public string? CompensationOperation { get; set; }
}
