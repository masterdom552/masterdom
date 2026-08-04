using System;
using System.Collections.Generic;
using Masterdom.Platform.Modules;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Converts module catalog entries into baseline workflow definitions.
/// </summary>
public static class WorkflowCatalogBuilder
{
    public static WorkflowCatalogSeed BuildFromCatalog(
        IEnumerable<ModuleCatalogEntry> entries,
        DateTime? effectiveFromUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var workflows = new List<WorkflowDefinition>();
        var versions = new List<WorkflowVersionDefinition>();
        var steps = new List<WorkflowStepDefinition>();
        var transitions = new List<WorkflowTransitionDefinition>();

        var fromUtc = effectiveFromUtc ?? DateTime.UnixEpoch;

        foreach (var entry in entries)
        {
            var workflowId = new WorkflowId(Guid.NewGuid());
            var versionId = new WorkflowVersionId(Guid.NewGuid());
            var startStep = new WorkflowStepId(Guid.NewGuid());
            var endStep = new WorkflowStepId(Guid.NewGuid());

            workflows.Add(new WorkflowDefinition(
                workflowId,
                new WorkflowKey($"workflow.{entry.ModuleId}.default"),
                $"{entry.Module.Metadata.DisplayName} Default Workflow",
                "Catalog-seeded default workflow.",
                WorkflowScope.Create(WorkflowScopeKind.Module, entry.ModuleId),
                "catalog",
                fromUtc));

            versions.Add(new WorkflowVersionDefinition(
                versionId,
                workflowId,
                new WorkflowVersion(1),
                new WorkflowEffectivePeriod(fromUtc, null),
                false,
                null,
                $"CatalogVersion:{entry.Version}",
                "catalog",
                fromUtc));

            steps.Add(new WorkflowStepDefinition(
                startStep,
                versionId,
                "start",
                "Start",
                WorkflowStepKind.Automatic,
                true,
                false,
                WorkflowRetryPolicy.None(),
                WorkflowTimeoutPolicy.None(),
                null));

            steps.Add(new WorkflowStepDefinition(
                endStep,
                versionId,
                "end",
                "End",
                WorkflowStepKind.Automatic,
                false,
                true,
                WorkflowRetryPolicy.None(),
                WorkflowTimeoutPolicy.None(),
                null));

            transitions.Add(new WorkflowTransitionDefinition(
                new WorkflowTransitionId(Guid.NewGuid()),
                versionId,
                startStep,
                endStep,
                WorkflowBranchKind.Sequential,
                new WorkflowPriority(1),
                WorkflowTransitionConditionKind.Always,
                null,
                null));
        }

        return new WorkflowCatalogSeed
        {
            Workflows = workflows,
            Versions = versions,
            Steps = steps,
            Transitions = transitions
        };
    }
}
