using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ScenarioGenerator
{
    public IReadOnlyList<SubsidyOptimizationScenario> Generate(
        SubsidyConsumptionEstimate estimate,
        SubsidyForecast forecast,
        SubsidyPolicyConfiguration policy,
        OptimizationStrategyConfiguration strategy,
        OptimizationModelConfiguration model)
    {
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(model);

        var mandatoryCandidates = policy.Slabs
            .Where(x => x.IsCliff)
            .SelectMany(boundary => new[]
            {
                decimal.Max(0m, boundary.MaximumUnits - model.BoundaryTolerance),
                boundary.MaximumUnits,
                boundary.MaximumUnits + model.BoundaryTolerance
            })
            .ToHashSet();
        if (mandatoryCandidates.Count > model.MaximumScenarioCount)
        {
            throw new InvalidOperationException("MaximumScenarioCount cannot represent every mandatory subsidy cliff candidate.");
        }

        var optionalCandidates = new HashSet<decimal>
        {
            decimal.Round(forecast.ProjectedConsumptionUnits, 4, MidpointRounding.AwayFromZero)
        };

        foreach (var factor in strategy.ConsumptionFactors)
        {
            optionalCandidates.Add(decimal.Round(forecast.ProjectedConsumptionUnits * factor, 4, MidpointRounding.AwayFromZero));
        }

        optionalCandidates.ExceptWith(mandatoryCandidates);
        var optionalLimit = model.MaximumScenarioCount - mandatoryCandidates.Count;
        var candidates = mandatoryCandidates
            .Concat(optionalCandidates.OrderBy(x => x).Take(optionalLimit))
            .Where(x => x >= 0m)
            .OrderBy(x => x)
            .ToArray();

        return candidates
            .Select((projected, index) => BuildScenario(index, estimate, projected, strategy))
            .ToArray();
    }

    private static SubsidyOptimizationScenario BuildScenario(
        int index,
        SubsidyConsumptionEstimate estimate,
        decimal projected,
        OptimizationStrategyConfiguration strategy)
    {
        var thresholdDelta = estimate.OccupancyAdjustedUnits - projected;

        return new SubsidyOptimizationScenario(
            ScenarioCode: $"candidate-{index + 1}",
            ScenarioName: $"Candidate {index + 1}",
            EstimatedConsumptionUnits: estimate.OccupancyAdjustedUnits,
            ForecastConsumptionUnits: projected,
            ExpectedSubsidy: 0m,
            ExpectedCost: 0m,
            SanctionedLoadImpact: 0m,
            ExpectedBenefit: 0m,
            ExpectedRisk: 0m,
            ThresholdDelta: thresholdDelta,
            SubsidyPreservationScore: 0m,
            IsFeasible: true,
            InfeasibilityReason: null,
            TriggeredBoundary: null,
            TradeOffSummary: string.Empty,
            RankScore: 0m,
            MeterAllocations: BuildAllocations(estimate.MeterEstimates, projected, strategy));
    }

    private static IReadOnlyList<SubsidyMeterAllocation> BuildAllocations(
        IReadOnlyList<SubsidyMeterEstimate> meters,
        decimal projected,
        OptimizationStrategyConfiguration strategy)
    {
        if (meters.Count == 0)
        {
            return [];
        }

        var baselineTotal = meters.Sum(x => x.BaselineUnits);
        var proportional = new decimal[meters.Count];
        var assigned = 0m;
        for (var index = 0; index < meters.Count; index++)
        {
            proportional[index] = index == meters.Count - 1
                ? projected - assigned
                : decimal.Round(
                    baselineTotal == 0m ? projected / meters.Count : projected * meters[index].BaselineUnits / baselineTotal,
                    4,
                    MidpointRounding.AwayFromZero);
            assigned += proportional[index];
        }

        var allocations = proportional.ToArray();
        if (strategy.PermitCrossMeterMovement)
        {
            var remainingMovementBudget = OptimizationAllocationInvariant.CalculateMovementBudget(
                projected,
                strategy.MaximumCrossMeterMovementFraction);
            for (var donorIndex = 0; donorIndex < meters.Count; donorIndex++)
            {
                if (remainingMovementBudget <= 0m)
                {
                    break;
                }

                var excess = decimal.Max(0m, allocations[donorIndex] - meters[donorIndex].SanctionedLoad);
                var movable = decimal.Min(excess, remainingMovementBudget);

                for (var recipientIndex = 0; recipientIndex < meters.Count && movable > 0m; recipientIndex++)
                {
                    if (recipientIndex == donorIndex)
                    {
                        continue;
                    }

                    var recipientCapacity = decimal.Max(
                        0m,
                        meters[recipientIndex].SanctionedLoad - allocations[recipientIndex]);
                    var transferred = decimal.Min(movable, recipientCapacity);
                    allocations[donorIndex] -= transferred;
                    allocations[recipientIndex] += transferred;
                    movable -= transferred;
                    remainingMovementBudget -= transferred;
                }
            }
        }

        return meters
            .Select((meter, index) => new SubsidyMeterAllocation(
                meter.MeterId,
                meter.BaselineUnits,
                allocations[index],
                meter.SanctionedLoad,
                allocations[index] - proportional[index]))
            .ToArray();
    }
}
