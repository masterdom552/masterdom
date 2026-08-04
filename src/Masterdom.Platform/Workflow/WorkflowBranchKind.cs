namespace Masterdom.Platform.Workflow;

/// <summary>
/// Defines supported workflow branch kinds.
/// </summary>
public enum WorkflowBranchKind
{
    Sequential = 0,
    Conditional = 1,
    Parallel = 2
}
