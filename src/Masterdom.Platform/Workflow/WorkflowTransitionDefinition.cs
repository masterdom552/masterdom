using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a workflow transition between two steps.
/// </summary>
public sealed class WorkflowTransitionDefinition
{
    public WorkflowTransitionDefinition(
        WorkflowTransitionId id,
        WorkflowVersionId workflowVersionId,
        WorkflowStepId fromStepId,
        WorkflowStepId toStepId,
        WorkflowBranchKind branchKind,
        WorkflowPriority priority,
        WorkflowTransitionConditionKind conditionKind,
        string? ruleSetKey,
        WorkflowScope? ruleScope)
    {
        if (fromStepId.Value == toStepId.Value)
        {
            throw new WorkflowValidationException("Workflow transition cannot target the same step.");
        }

        if (conditionKind == WorkflowTransitionConditionKind.Rule && string.IsNullOrWhiteSpace(ruleSetKey))
        {
            throw new WorkflowValidationException("Rule-based transitions require a rule set key.");
        }

        Id = id;
        WorkflowVersionId = workflowVersionId;
        FromStepId = fromStepId;
        ToStepId = toStepId;
        BranchKind = branchKind;
        Priority = priority;
        ConditionKind = conditionKind;
        RuleSetKey = string.IsNullOrWhiteSpace(ruleSetKey) ? null : new WorkflowKey(ruleSetKey);
        RuleScope = ruleScope;
    }

    public WorkflowTransitionId Id { get; }

    public WorkflowVersionId WorkflowVersionId { get; }

    public WorkflowStepId FromStepId { get; }

    public WorkflowStepId ToStepId { get; }

    public WorkflowBranchKind BranchKind { get; }

    public WorkflowPriority Priority { get; }

    public WorkflowTransitionConditionKind ConditionKind { get; }

    public WorkflowKey? RuleSetKey { get; }

    public WorkflowScope? RuleScope { get; }
}
