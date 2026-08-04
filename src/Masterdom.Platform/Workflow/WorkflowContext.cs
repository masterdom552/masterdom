using System;
using Masterdom.Platform.Rules;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents runtime context for workflow execution.
/// </summary>
public sealed class WorkflowContext
{
    public required string ModuleId { get; init; }

    public string? TenantId { get; init; }

    public string? PropertyId { get; init; }

    public DateTime AsOfUtc { get; init; }

    public string? CorrelationId { get; init; }

    public bool CancellationRequested { get; init; }

    public RuleInput Input { get; init; } = new RuleInput(Array.Empty<RuleInputItem>());
}
