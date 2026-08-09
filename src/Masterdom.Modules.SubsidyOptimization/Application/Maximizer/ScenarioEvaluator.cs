using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ScenarioEvaluator
{
    private readonly SubsidyCalculationRuntimeInvoker _runtime;

    public ScenarioEvaluator(SubsidyCalculationRuntimeInvoker runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public IReadOnlyList<SubsidyOptimizationScenario> RankScenarios(
        IReadOnlyCollection<SubsidyOptimizationScenario> scenarios,
        SubsidyPolicyConfiguration policy,
        OptimizationModelConfiguration model,
        OptimizationStrategyConfiguration strategy,
        IReadOnlyCollection<RatedConsumptionContract> ratedConsumptions,
        DateTime effectiveDateUtc)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(ratedConsumptions);

        var scoredScenarios = scenarios
            .Select(x => Evaluate(x, policy, model, strategy, ratedConsumptions, effectiveDateUtc))
            .ToArray();

        var preliminaryOrder = SubsidyCalculationRuntimeInvoker.ReadIntList(
            _runtime.Execute(
                "ranking.tie_break",
                new Dictionary<string, object?>
                {
                    ["primary_scores"] = scoredScenarios.Select(x => x.RankScore).ToArray(),
                    ["secondary_scores"] = scoredScenarios.Select(x => x.SubsidyPreservationScore).ToArray()
                },
                effectiveDateUtc),
            "ordered_indices");

        var ordered = new List<SubsidyOptimizationScenario>(scoredScenarios.Length);
        var index = 0;

        while (index < preliminaryOrder.Count)
        {
            var current = scoredScenarios[preliminaryOrder[index]];
            var tieGroup = new List<int> { preliminaryOrder[index] };
            index++;

            while (index < preliminaryOrder.Count)
            {
                var candidate = scoredScenarios[preliminaryOrder[index]];
                if (candidate.RankScore != current.RankScore
                    || candidate.SubsidyPreservationScore != current.SubsidyPreservationScore)
                {
                    break;
                }

                tieGroup.Add(preliminaryOrder[index]);
                index++;
            }

            if (tieGroup.Count == 1)
            {
                ordered.Add(scoredScenarios[tieGroup[0]]);
                continue;
            }

            var tieBreakOrder = SubsidyCalculationRuntimeInvoker.ReadIntList(
                _runtime.Execute(
                    "ranking.tie_break",
                    new Dictionary<string, object?>
                    {
                        ["primary_scores"] = tieGroup.Select(_ => 0m).ToArray(),
                        ["secondary_scores"] = tieGroup.Select(groupIndex => -scoredScenarios[groupIndex].ExpectedRisk).ToArray()
                    },
                    effectiveDateUtc),
                "ordered_indices");

            ordered.AddRange(tieBreakOrder.Select(localIndex => scoredScenarios[tieGroup[localIndex]]));
        }

        return ordered
            .OrderByDescending(x => x.IsFeasible)
            .ThenByDescending(x => x.RankScore)
            .ThenBy(x => x.ForecastConsumptionUnits)
            .ToArray();
    }

    private SubsidyOptimizationScenario Evaluate(
        SubsidyOptimizationScenario scenario,
        SubsidyPolicyConfiguration policy,
        OptimizationModelConfiguration model,
        OptimizationStrategyConfiguration strategy,
        IReadOnlyCollection<RatedConsumptionContract> ratedConsumptions,
        DateTime effectiveDateUtc)
    {
        var slab = policy.Slabs
            .OrderBy(x => x.MaximumUnits)
            .FirstOrDefault(x => scenario.ForecastConsumptionUnits <= x.MaximumUnits);
        var expectedSubsidy = slab?.SubsidyAmount ?? 0m;
        var individualLoadViolation = scenario.MeterAllocations.Sum(
            x => decimal.Max(0m, x.AllocatedUnits - x.SanctionedLoad));
        var transferredUnits = OptimizationAllocationInvariant.CalculateTransferredUnits(
            scenario.MeterAllocations.Select(x => x.MovementUnits));
        var propertyLoadImpact = decimal.Max(0m, scenario.ForecastConsumptionUnits - policy.SanctionedLoadLimit);
        var sanctionedLoadImpact = individualLoadViolation + transferredUnits + propertyLoadImpact;
        var ratedUnits = ratedConsumptions.Sum(x => x.RatedUnits);
        var ratedAmount = ratedConsumptions.Sum(x => x.RatedAmount);
        var ratedUnitCost = ratedUnits <= 0m ? 0m : ratedAmount / ratedUnits;
        var expectedCost = scenario.ForecastConsumptionUnits * ratedUnitCost
            + sanctionedLoadImpact * policy.SanctionedLoadPenaltyPerUnit;
        var allocationsConserved = OptimizationAllocationInvariant.IsConserved(
            scenario.ForecastConsumptionUnits,
            scenario.MeterAllocations.Select(x => x.AllocatedUnits),
            model.BoundaryTolerance);
        var movementsConserved = OptimizationAllocationInvariant.IsMovementConserved(
            scenario.MeterAllocations.Select(x => x.MovementUnits),
            model.BoundaryTolerance);
        var movementWithinBudget = OptimizationAllocationInvariant.IsWithinMovementBudget(
            scenario.ForecastConsumptionUnits,
            strategy.MaximumCrossMeterMovementFraction,
            scenario.MeterAllocations.Select(x => x.MovementUnits),
            model.BoundaryTolerance);
        var movementAllowed = strategy.PermitCrossMeterMovement
            || scenario.MeterAllocations.All(x => x.MovementUnits == 0m);
        var individualLoadsValid = scenario.MeterAllocations.All(x => x.AllocatedUnits >= 0m && x.AllocatedUnits <= x.SanctionedLoad);
        var isFeasible = scenario.ForecastConsumptionUnits >= 0m
            && allocationsConserved
            && movementsConserved
            && movementWithinBudget
            && movementAllowed
            && individualLoadsValid;
        var risk = decimal.Abs(scenario.ForecastConsumptionUnits - scenario.EstimatedConsumptionUnits);
        var preservation = expectedSubsidy <= 0m ? 0m : expectedSubsidy / decimal.Max(expectedSubsidy + expectedCost, 1m);

        var result = _runtime.Execute(
            "scoring.weighted_score",
            new Dictionary<string, object?>
            {
                ["values"] = new[]
                {
                    expectedSubsidy,
                    -expectedCost,
                    -sanctionedLoadImpact,
                    -risk
                },
                ["weights"] = new[]
                {
                    model.SubsidyWeight,
                    model.CostWeight,
                    model.LoadImpactWeight,
                    model.RiskWeight
                }
            },
            effectiveDateUtc);

        var score = isFeasible ? SubsidyCalculationRuntimeInvoker.ReadDecimal(result, "value") : decimal.MinValue;
        return scenario with
        {
            ExpectedSubsidy = expectedSubsidy,
            ExpectedCost = expectedCost,
            SanctionedLoadImpact = sanctionedLoadImpact,
            ExpectedBenefit = expectedSubsidy,
            ExpectedRisk = risk,
            SubsidyPreservationScore = preservation,
            IsFeasible = isFeasible,
            InfeasibilityReason = isFeasible ? null : "Scenario violates property conservation, movement, or sanctioned-load constraints.",
            TriggeredBoundary = slab?.MaximumUnits,
            TradeOffSummary = $"subsidy={expectedSubsidy:F2};cost={expectedCost:F2};loadImpact={sanctionedLoadImpact:F2};risk={risk:F2}",
            RankScore = score
        };
    }
}
