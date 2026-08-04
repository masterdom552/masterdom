using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record CompleteOptimizationCommand(
    OptimizationRunId OptimizationRunId,
    OptimizationResult OptimizationResult,
    ConsumptionForecast ConsumptionForecast,
    IReadOnlyCollection<OptimizationRecommendation> Recommendations,
    DateTime CompletedAtUtc)
{
    public RecommendationSet ToRecommendationSet()
    {
        return RecommendationSet.Create(Recommendations);
    }
}
