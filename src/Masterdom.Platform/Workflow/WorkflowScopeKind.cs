namespace Masterdom.Platform.Workflow;

/// <summary>
/// Defines scope kinds for workflows.
/// </summary>
public enum WorkflowScopeKind
{
    Global = 0,
    Module = 1,
    Tenant = 2,
    Aggregate = 3,
    Entity = 4
}
