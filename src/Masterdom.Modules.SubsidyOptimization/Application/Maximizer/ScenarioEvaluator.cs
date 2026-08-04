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
        DateTime effectiveDateUtc)
    {
        ArgumentNullException.ThrowIfNull(scenarios);

        var scoredScenarios = scenarios
            .Select(x => x with { RankScore = CalculateScore(x, effectiveDateUtc) })
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

        return ordered;
    }

    private decimal CalculateScore(SubsidyOptimizationScenario scenario, DateTime effectiveDateUtc)
    {
        var result = _runtime.Execute(
            "scoring.weighted_score",
            new Dictionary<string, object?>
            {
                ["values"] = new[]
                {
                    scenario.ExpectedBenefit,
                    -scenario.ExpectedRisk,
                    scenario.SubsidyPreservationScore,
                    scenario.ThresholdDelta
                },
                ["weights"] = new[] { 2.5m, 1.7m, 10m, 0.1m }
            },
            effectiveDateUtc);

        return SubsidyCalculationRuntimeInvoker.ReadDecimal(result, "value");
    }
}
