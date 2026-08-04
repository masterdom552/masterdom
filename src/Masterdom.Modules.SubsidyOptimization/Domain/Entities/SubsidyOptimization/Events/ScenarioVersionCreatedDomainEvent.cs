using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;

public sealed record ScenarioVersionCreatedDomainEvent(
    OptimizationRunId PreviousRunId,
    OptimizationRunId NewRunId,
    string ScenarioId,
    int Version,
    DateTime OccurredOnUtc) : IDomainEvent;
