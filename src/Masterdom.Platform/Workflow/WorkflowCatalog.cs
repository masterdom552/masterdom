using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Immutable workflow catalog.
/// </summary>
public sealed class WorkflowCatalog : IWorkflowCatalog
{
    public WorkflowCatalog(
        IEnumerable<WorkflowDefinition> workflows,
        IEnumerable<WorkflowVersionDefinition> versions,
        IEnumerable<WorkflowStepDefinition> steps,
        IEnumerable<WorkflowTransitionDefinition> transitions)
    {
        ArgumentNullException.ThrowIfNull(workflows);
        ArgumentNullException.ThrowIfNull(versions);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(transitions);

        Workflows = workflows.ToList();
        Versions = versions.ToList();
        Steps = steps.ToList();
        Transitions = transitions.ToList();
    }

    public IReadOnlyList<WorkflowDefinition> Workflows { get; }

    public IReadOnlyList<WorkflowVersionDefinition> Versions { get; }

    public IReadOnlyList<WorkflowStepDefinition> Steps { get; }

    public IReadOnlyList<WorkflowTransitionDefinition> Transitions { get; }
}
