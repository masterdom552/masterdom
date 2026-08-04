using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record ArchiveRecommendationCommand(
    OptimizationRunId OptimizationRunId,
    RecommendationId RecommendationId,
    string Reason,
    DateTime ArchivedAtUtc);
