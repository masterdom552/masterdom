using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ConfidenceScorer
{
    private readonly SubsidyCalculationRuntimeInvoker _runtime;

    public ConfidenceScorer(SubsidyCalculationRuntimeInvoker runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public RecommendationConfidence Score(
        SubsidyConsumptionEstimate estimate,
        IReadOnlyCollection<SubsidyOptimizationScenario> scenarios,
        decimal minimumThreshold,
        DateTime effectiveDateUtc)
    {
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(scenarios);

        var boundedThreshold = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.clamp",
                new Dictionary<string, object?>
                {
                    ["value"] = minimumThreshold,
                    ["min"] = 0m,
                    ["max"] = 1m
                },
                effectiveDateUtc),
            "value");

        var spread = scenarios.Count <= 1
            ? 0m
            : SubsidyCalculationRuntimeInvoker.ReadDecimal(
                _runtime.Execute(
                    "statistics.spread",
                    new Dictionary<string, object?>
                    {
                        ["values"] = scenarios.Select(x => x.ForecastConsumptionUnits).ToArray()
                    },
                    effectiveDateUtc),
                "value");

        var spreadPenaltyRatio = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.ratio",
                new Dictionary<string, object?>
                {
                    ["numerator"] = spread,
                    ["denominator"] = 100m
                },
                effectiveDateUtc),
            "value");

        var spreadPenalty = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.clamp",
                new Dictionary<string, object?>
                {
                    ["value"] = spreadPenaltyRatio,
                    ["min"] = 0m,
                    ["max"] = 0.4m
                },
                effectiveDateUtc),
            "value");

        var confidence = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "scoring.confidence",
                new Dictionary<string, object?>
                {
                    ["quality"] = estimate.DataCompletenessRatio,
                    ["penalty"] = spreadPenalty,
                    ["min"] = boundedThreshold,
                    ["max"] = 0.99m
                },
                effectiveDateUtc),
            "value");

        return RecommendationConfidence.Create(confidence);
    }
}
