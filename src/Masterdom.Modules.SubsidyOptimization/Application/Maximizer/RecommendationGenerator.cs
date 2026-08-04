using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class RecommendationGenerator
{
    private readonly RecommendationExplanationBuilder _explanationBuilder;
    private readonly RecommendationEvidenceBuilder _evidenceBuilder;

    public RecommendationGenerator(
        RecommendationExplanationBuilder explanationBuilder,
        RecommendationEvidenceBuilder evidenceBuilder)
    {
        _explanationBuilder = explanationBuilder ?? throw new ArgumentNullException(nameof(explanationBuilder));
        _evidenceBuilder = evidenceBuilder ?? throw new ArgumentNullException(nameof(evidenceBuilder));
    }

    public IReadOnlyList<Recommendation> Generate(
        IReadOnlyCollection<SubsidyOptimizationScenario> rankedScenarios,
        SubsidyConsumptionEstimate estimate,
        SubsidyForecast forecast,
        RecommendationConfidence confidence,
        IReadOnlyCollection<ImportedDatasetReference> importedDatasets,
        IReadOnlyDictionary<string, string> configurationVersions,
        DateTime effectiveDateUtc,
        string optimizationModel,
        string optimizationStrategy,
        string effectivePolicy)
    {
        ArgumentNullException.ThrowIfNull(rankedScenarios);
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(confidence);
        ArgumentNullException.ThrowIfNull(importedDatasets);
        ArgumentNullException.ThrowIfNull(configurationVersions);

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("effectiveDateUtc must be UTC.");
        }

        var nowUtc = DateTime.UtcNow;
        var topScenarios = rankedScenarios.Take(3).ToArray();
        var recommendations = new List<Recommendation>(topScenarios.Length);

        for (var index = 0; index < topScenarios.Length; index++)
        {
            var scenario = topScenarios[index];
            var priority = RecommendationPriority.Create(index + 1);
            var explanation = _explanationBuilder.Build(
                scenario,
                estimate,
                forecast,
                confidence,
                configurationVersions,
                optimizationModel,
                optimizationStrategy,
                effectivePolicy);
            var evidence = _evidenceBuilder.Build(
                scenario,
                estimate,
                forecast,
                confidence,
                importedDatasets,
                configurationVersions,
                optimizationModel,
                optimizationStrategy,
                effectivePolicy);

            var metadata = new RecommendationMetadata(
                createdAtUtc: nowUtc,
                effectiveDateUtc: effectiveDateUtc,
                version: "subsidy-maximizer-v1",
                source: "subsidy-optimization",
                attributes: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["scenario"] = scenario.ScenarioCode,
                    ["expected_benefit"] = scenario.ExpectedBenefit.ToString("F2"),
                    ["expected_risk"] = scenario.ExpectedRisk.ToString("F2"),
                    ["trade_offs"] = scenario.TradeOffSummary,
                    ["configuration_version"] = string.Join(";", configurationVersions.Select(x => $"{x.Key}:{x.Value}")),
                    ["optimization_model"] = optimizationModel,
                    ["optimization_strategy"] = optimizationStrategy,
                    ["effective_policy"] = effectivePolicy
                });

            var recommendation = Recommendation
                .CreateDraft(
                    RecommendationId.New(),
                    RecommendationType.Create("subsidy-maximizer"),
                    priority,
                    confidence,
                    evidence,
                    explanation,
                    metadata)
                .MarkProposed(nowUtc);

            recommendations.Add(recommendation);
        }

        return recommendations;
    }
}
