using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;

public sealed record RecommendationGeneratedDomainEvent(
    OptimizationRunId OptimizationRunId,
    Guid RecommendationId,
    string Priority,
    DateTime OccurredOnUtc) : IDomainEvent;
