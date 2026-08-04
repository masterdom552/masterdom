using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class RecommendationEvidenceBuilder
{
    public RecommendationEvidence Build(
        SubsidyOptimizationScenario scenario,
        SubsidyConsumptionEstimate estimate,
        SubsidyForecast forecast,
        RecommendationConfidence confidence,
        IReadOnlyCollection<ImportedDatasetReference> importedDatasets,
        IReadOnlyDictionary<string, string> configurationVersions,
        string optimizationModel,
        string optimizationStrategy,
        string effectivePolicy)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(importedDatasets);
        ArgumentNullException.ThrowIfNull(configurationVersions);

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["summary"] = scenario.ScenarioName,
            ["expected_benefit"] = scenario.ExpectedBenefit.ToString("F2"),
            ["expected_risk"] = scenario.ExpectedRisk.ToString("F2"),
            ["trade_offs"] = scenario.TradeOffSummary,
            ["assumptions"] = $"occupancy={estimate.OccupancyAdjustedUnits:F2};trend={forecast.TrendFactor:F4}",
            ["confidence"] = confidence.Value.ToString("F4"),
            ["optimization_model"] = optimizationModel,
            ["strategy"] = optimizationStrategy,
            ["effective_policy"] = effectivePolicy,
            ["imported_dataset_count"] = importedDatasets.Count.ToString(),
            ["configuration_versions"] = string.Join(";", configurationVersions.Select(x => $"{x.Key}:{x.Value}"))
        };

        return new RecommendationEvidence(
            code: $"SUBSIDY-{scenario.ScenarioCode.ToUpperInvariant()}",
            detail: $"Scenario {scenario.ScenarioName} evaluated for subsidy preservation and threshold impact.",
            attributes: attributes);
    }
}
