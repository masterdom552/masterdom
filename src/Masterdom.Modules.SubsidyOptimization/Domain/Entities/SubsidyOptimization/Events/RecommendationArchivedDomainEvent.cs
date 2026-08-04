using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;

public sealed record RecommendationArchivedDomainEvent(
    OptimizationRunId OptimizationRunId,
    Guid RecommendationId,
    DateTime OccurredOnUtc) : IDomainEvent;
