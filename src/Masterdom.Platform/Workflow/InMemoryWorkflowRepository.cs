using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// In-memory workflow repository implementation.
/// </summary>
public sealed class InMemoryWorkflowRepository : IWorkflowRepository
{
    private List<WorkflowDefinition> _workflows;
    private List<WorkflowVersionDefinition> _versions;
    private List<WorkflowStepDefinition> _steps;
    private List<WorkflowTransitionDefinition> _transitions;

    public InMemoryWorkflowRepository(
        IReadOnlyList<WorkflowDefinition>? workflows = null,
        IReadOnlyList<WorkflowVersionDefinition>? versions = null,
        IReadOnlyList<WorkflowStepDefinition>? steps = null,
        IReadOnlyList<WorkflowTransitionDefinition>? transitions = null)
    {
        _workflows = workflows?.ToList() ?? new List<WorkflowDefinition>();
        _versions = versions?.ToList() ?? new List<WorkflowVersionDefinition>();
        _steps = steps?.ToList() ?? new List<WorkflowStepDefinition>();
        _transitions = transitions?.ToList() ?? new List<WorkflowTransitionDefinition>();
    }

    public IReadOnlyList<WorkflowDefinition> GetAllWorkflows() => _workflows;

    public IReadOnlyList<WorkflowVersionDefinition> GetAllVersions() => _versions;

    public IReadOnlyList<WorkflowStepDefinition> GetAllSteps() => _steps;

    public IReadOnlyList<WorkflowTransitionDefinition> GetAllTransitions() => _transitions;

    public void ReplaceAll(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        _workflows = workflows?.ToList() ?? throw new ArgumentNullException(nameof(workflows));
        _versions = versions?.ToList() ?? throw new ArgumentNullException(nameof(versions));
        _steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps));
        _transitions = transitions?.ToList() ?? throw new ArgumentNullException(nameof(transitions));
    }
}
