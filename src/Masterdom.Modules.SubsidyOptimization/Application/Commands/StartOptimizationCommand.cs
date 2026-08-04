using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record StartOptimizationCommand(
    SubsidyScenario Scenario,
    MeterGroup MeterGroup,
    OptimizationPeriod OptimizationPeriod,
    IReadOnlyCollection<MeteringConsumptionHistoryContract> ConsumptionHistory,
    IReadOnlyCollection<RatedConsumptionContract> RatedConsumptions)
{
    public RatingReference ToRatingReference()
    {
        var ids = RatedConsumptions.Select(x => x.RatingId).Distinct().ToArray();
        return RatingReference.Create(ids);
    }
}
