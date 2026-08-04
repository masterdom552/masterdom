using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents seeded workflow data from module catalog.
/// </summary>
public sealed class WorkflowCatalogSeed
{
    public required IReadOnlyList<WorkflowDefinition> Workflows { get; init; }

    public required IReadOnlyList<WorkflowVersionDefinition> Versions { get; init; }

    public required IReadOnlyList<WorkflowStepDefinition> Steps { get; init; }

    public required IReadOnlyList<WorkflowTransitionDefinition> Transitions { get; init; }
}
