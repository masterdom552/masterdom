using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Default workflow registry implementation.
/// </summary>
public sealed class WorkflowRegistry : IWorkflowRegistry
{
    private readonly InMemoryWorkflowRepository _repository;

    public WorkflowRegistry(IWorkflowRepository? repository = null)
    {
        _repository = repository as InMemoryWorkflowRepository
            ?? new InMemoryWorkflowRepository(
                repository?.GetAllWorkflows(),
                repository?.GetAllVersions(),
                repository?.GetAllSteps(),
                repository?.GetAllTransitions());
    }

    public void ReplaceAll(
        IReadOnlyList<WorkflowDefinition> workflows,
        IReadOnlyList<WorkflowVersionDefinition> versions,
        IReadOnlyList<WorkflowStepDefinition> steps,
        IReadOnlyList<WorkflowTransitionDefinition> transitions)
    {
        ArgumentNullException.ThrowIfNull(workflows);
        ArgumentNullException.ThrowIfNull(versions);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(transitions);

        WorkflowValidation.ValidateAll(workflows, versions, steps, transitions);
        _repository.ReplaceAll(workflows, versions, steps, transitions);
    }

    public IWorkflowCatalog GetCatalog()
    {
        return new WorkflowCatalog(
            _repository.GetAllWorkflows(),
            _repository.GetAllVersions(),
            _repository.GetAllSteps(),
            _repository.GetAllTransitions());
    }
}
