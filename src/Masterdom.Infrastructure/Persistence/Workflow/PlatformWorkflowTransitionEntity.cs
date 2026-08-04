using System;

namespace Masterdom.Infrastructure.Persistence.Workflow;

/// <summary>
/// Persistence model for workflow transitions.
/// </summary>
public sealed class PlatformWorkflowTransitionEntity
{
    public Guid Id { get; set; }

    public Guid WorkflowVersionId { get; set; }

    public Guid FromStepId { get; set; }

    public Guid ToStepId { get; set; }

    public int BranchKind { get; set; }

    public int Priority { get; set; }

    public int ConditionKind { get; set; }

    public string? RuleSetKey { get; set; }

    public int? RuleScopeKind { get; set; }

    public string? RuleScopeIdentifier { get; set; }
}
