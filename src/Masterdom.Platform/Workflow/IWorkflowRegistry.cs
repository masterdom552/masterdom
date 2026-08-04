using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Registers workflow definitions in runtime catalog.
/// </summary>
public interface IWorkflowRegistry
{
    void ReplaceAll(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions);

    IWorkflowCatalog GetCatalog();
}
