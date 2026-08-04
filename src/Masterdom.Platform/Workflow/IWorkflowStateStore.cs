namespace Masterdom.Platform.Workflow;

/// <summary>
/// Persists workflow execution states.
/// </summary>
public interface IWorkflowStateStore
{
    void Save(WorkflowState state);

    bool TryGet(System.Guid instanceId, out WorkflowState? state);
}
