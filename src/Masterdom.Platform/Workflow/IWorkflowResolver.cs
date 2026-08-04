namespace Masterdom.Platform.Workflow;

/// <summary>
/// Resolves and executes effective workflows.
/// </summary>
public interface IWorkflowResolver
{
    WorkflowResult Execute(WorkflowKey workflowKey, WorkflowScope scope, WorkflowContext context);
}
