using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents an immutable runtime workflow catalog.
/// </summary>
public interface IWorkflowCatalog
{
    IReadOnlyList<WorkflowDefinition> Workflows { get; }

    IReadOnlyList<WorkflowVersionDefinition> Versions { get; }

    IReadOnlyList<WorkflowStepDefinition> Steps { get; }

    IReadOnlyList<WorkflowTransitionDefinition> Transitions { get; }
}
