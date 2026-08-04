using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class RecommendationExplanationBuilder
{
    public RecommendationExplanation Build(
        SubsidyOptimizationScenario scenario,
        SubsidyConsumptionEstimate estimate,
        SubsidyForecast forecast,
        RecommendationConfidence confidence,
        IReadOnlyDictionary<string, string> configurationVersions,
        string optimizationModel,
        string optimizationStrategy,
        string effectivePolicy)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(configurationVersions);

        var summary = $"{scenario.ScenarioName} projects {scenario.ForecastConsumptionUnits:F2} units with expected benefit {scenario.ExpectedBenefit:F2} and risk {scenario.ExpectedRisk:F2}.";

        var assumptions = $"Assumptions: occupancy-adjusted estimate {estimate.OccupancyAdjustedUnits:F2}, trend factor {forecast.TrendFactor:F4}.";
        var confidenceLine = $"Confidence: {confidence.Value:P1} based on completeness {estimate.DataCompletenessRatio:P1}.";
        var tradeOffs = $"Trade-offs: {scenario.TradeOffSummary}.";
        var reproducibility = $"Configuration versions: {string.Join(", ", configurationVersions.Select(x => $"{x.Key}={x.Value}"))}.";
        var strategy = $"Optimization model: {optimizationModel}. Strategy: {optimizationStrategy}. Effective policy: {effectivePolicy}.";

        return new RecommendationExplanation(
            summary,
            [assumptions, confidenceLine, tradeOffs, reproducibility, strategy]);
    }
}
