using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record CreateScenarioVersionCommand(
    OptimizationRunId OptimizationRunId,
    IReadOnlyCollection<RatedConsumptionContract> RatedConsumptions,
    DateTime StartedAtUtc)
{
    public RatingReference ToRatingReference()
    {
        var ids = RatedConsumptions.Select(x => x.RatingId).Distinct().ToArray();
        return RatingReference.Create(ids);
    }
}
