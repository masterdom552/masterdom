using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;

public sealed record OptimizationCompletedDomainEvent(
    OptimizationRunId OptimizationRunId,
    string ScenarioId,
    int Version,
    DateTime OccurredOnUtc) : IDomainEvent;
