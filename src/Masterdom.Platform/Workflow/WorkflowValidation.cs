using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Performs workflow-model validation.
/// </summary>
public static class WorkflowValidation
{
    public static void ValidateAll(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        ValidateDuplicates(workflows, versions, steps, transitions);
        ValidateReferences(workflows, versions, steps, transitions);
        ValidateVersionGraphs(versions, steps, transitions);
    }

    private static void ValidateDuplicates(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        if (workflows.GroupBy(x => x.Id.Value).Any(x => x.Count() > 1))
        {
            throw new WorkflowValidationException("Duplicate workflow identifiers were found.");
        }

        if (versions.GroupBy(x => x.Id.Value).Any(x => x.Count() > 1))
        {
            throw new WorkflowValidationException("Duplicate workflow version identifiers were found.");
        }

        if (steps.GroupBy(x => x.Id.Value).Any(x => x.Count() > 1))
        {
            throw new WorkflowValidationException("Duplicate workflow step identifiers were found.");
        }

        if (transitions.GroupBy(x => x.Id.Value).Any(x => x.Count() > 1))
        {
            throw new WorkflowValidationException("Duplicate workflow transition identifiers were found.");
        }

        if (workflows.GroupBy(x => new { Key = x.Key.Value.ToUpperInvariant(), Scope = x.Scope.Kind, Id = (x.Scope.Identifier ?? string.Empty).ToUpperInvariant() }).Any(x => x.Count() > 1))
        {
            throw new WorkflowValidationException("Duplicate workflow key and scope combinations were found.");
        }
    }

    private static void ValidateReferences(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        var workflowIds = workflows.Select(x => x.Id.Value).ToHashSet();
        var versionIds = versions.Select(x => x.Id.Value).ToHashSet();
        var stepIds = steps.Select(x => x.Id.Value).ToHashSet();

        foreach (var version in versions)
        {
            if (!workflowIds.Contains(version.WorkflowId.Value))
            {
                throw new WorkflowValidationException("Workflow version references missing workflow.");
            }
        }

        foreach (var step in steps)
        {
            if (!versionIds.Contains(step.WorkflowVersionId.Value))
            {
                throw new WorkflowValidationException("Workflow step references missing workflow version.");
            }
        }

        foreach (var transition in transitions)
        {
            if (!versionIds.Contains(transition.WorkflowVersionId.Value))
            {
                throw new WorkflowValidationException("Workflow transition references missing workflow version.");
            }

            if (!stepIds.Contains(transition.FromStepId.Value) || !stepIds.Contains(transition.ToStepId.Value))
            {
                throw new WorkflowValidationException("Workflow transition references missing step.");
            }

            if (transition.ConditionKind == WorkflowTransitionConditionKind.Rule && transition.RuleSetKey is null)
            {
                throw new WorkflowValidationException("Rule-based transition is missing rule set reference.");
            }
        }
    }

    private static void ValidateVersionGraphs(
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        foreach (var version in versions)
        {
            var vSteps = steps.Where(x => x.WorkflowVersionId.Value == version.Id.Value).ToList();
            var vTransitions = transitions.Where(x => x.WorkflowVersionId.Value == version.Id.Value).ToList();

            if (vSteps.Count == 0)
            {
                throw new WorkflowValidationException("Workflow version has no steps.");
            }

            if (vSteps.Count(x => x.IsStart) != 1)
            {
                throw new WorkflowValidationException("Workflow version must define exactly one start step.");
            }

            if (!vSteps.Any(x => x.IsTerminal))
            {
                throw new WorkflowValidationException("Workflow version must define at least one terminal step.");
            }

            if (vSteps.GroupBy(x => x.Key.ToUpperInvariant()).Any(x => x.Count() > 1))
            {
                throw new WorkflowValidationException("Duplicate step keys were found in workflow version.");
            }

            var fromGroups = vTransitions.GroupBy(x => x.FromStepId.Value).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var step in vSteps.Where(x => !x.IsTerminal))
            {
                if (fromGroups.ContainsKey(step.Id.Value))
                {
                    continue;
                }

                throw new WorkflowValidationException("Non-terminal workflow step has no outgoing transition.");
            }

            var visited = new HashSet<Guid>();
            var stack = new HashSet<Guid>();

            bool HasCycle(Guid stepId)
            {
                if (!visited.Add(stepId))
                {
                    return false;
                }

                stack.Add(stepId);

                if (fromGroups.TryGetValue(stepId, out var outs))
                {
                    foreach (var edge in outs)
                    {
                        if (stack.Contains(edge.ToStepId.Value))
                        {
                            return true;
                        }

                        if (HasCycle(edge.ToStepId.Value))
                        {
                            return true;
                        }
                    }
                }

                stack.Remove(stepId);
                return false;
            }

            foreach (var step in vSteps)
            {
                if (HasCycle(step.Id.Value))
                {
                    throw new WorkflowValidationException("Circular workflow transitions were found.");
                }
            }
        }
    }
}
