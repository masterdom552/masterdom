using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Provides read access to workflow definitions.
/// </summary>
public interface IWorkflowRepository
{
    IReadOnlyList<WorkflowDefinition> GetAllWorkflows();

    IReadOnlyList<WorkflowVersionDefinition> GetAllVersions();

    IReadOnlyList<WorkflowStepDefinition> GetAllSteps();

    IReadOnlyList<WorkflowTransitionDefinition> GetAllTransitions();
}
