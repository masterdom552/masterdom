using System.Text.Json;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Platform.CalculationEngine;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Recommendation;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Core.Tests.SubsidyOptimization;

public sealed class SubsidyMaximizerServiceTests
{
    [Fact]
    public void Execute_ShouldPerformHistoricalAnalysisAndForecast()
    {
        var service = CreateService();
        var request = CreateRequest();

        var result = service.Execute(request);

        Assert.True(result.ConsumptionEstimate.HistoricalAverageUnits > 0m);
        Assert.True(result.ConsumptionEstimate.WeightedAverageUnits > 0m);
        Assert.True(result.Forecast.ProjectedConsumptionUnits > 0m);
        Assert.NotEmpty(result.RankedScenarios);
    }

    [Fact]
    public void Execute_ShouldEstimateFailedMeterConsumption()
    {
        var service = CreateService();
        var request = CreateRequest(
            consumptionHistory:
            [
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 0m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 0m, DateTime.UtcNow)
            ]);

        var result = service.Execute(request);

        Assert.True(result.ConsumptionEstimate.FailedMeterEstimateUnits > 0m);
        Assert.True(result.ConsumptionEstimate.DataCompletenessRatio < 1m);
    }

    [Fact]
    public void Execute_ShouldGenerateAndRankScenarios()
    {
        var service = CreateService();

        var result = service.Execute(CreateRequest());

        Assert.True(result.RankedScenarios.Count >= 3);
        Assert.True(result.RankedScenarios[0].RankScore >= result.RankedScenarios[^1].RankScore);
    }

    [Fact]
    public void Execute_ShouldScoreConfidenceWithinThreshold()
    {
        var service = CreateService();
        var request = CreateRequest(confidenceThreshold: 0.62m);

        var result = service.Execute(request);

        Assert.True(result.Confidence.Value >= 0.62m);
        Assert.True(result.Confidence.Value <= 0.99m);
    }

    [Fact]
    public void Execute_ShouldGenerateRecommendationBundleWithExplainability()
    {
        var service = CreateService();

        var result = service.Execute(CreateRequest());

        Assert.Equal(RecommendationBundleStatus.Finalized, result.RecommendationBundle.Status);
        Assert.NotEmpty(result.RecommendationBundle.Recommendations);

        var recommendation = result.RecommendationBundle.Recommendations[0];

        Assert.False(string.IsNullOrWhiteSpace(recommendation.Explanation.Summary));
        Assert.NotEmpty(recommendation.Explanation.ReasoningSteps);
        Assert.False(string.IsNullOrWhiteSpace(recommendation.Evidence.Detail));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("assumptions"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("expected_benefit"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("expected_risk"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("trade_offs"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("configuration_versions"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("optimization_model"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("strategy"));
        Assert.True(recommendation.Evidence.Attributes.ContainsKey("effective_policy"));
    }

    [Fact]
    public void Execute_ShouldCreateReproducibleOptimizationSession()
    {
        var service = CreateService();

        var result = service.Execute(CreateRequest());

        Assert.Equal(OptimizationSessionStatus.Completed, result.OptimizationSession.Status);

        var attributes = result.OptimizationSession.Metadata.Attributes;
        Assert.True(attributes.ContainsKey("business_context_version"));
        Assert.True(attributes.ContainsKey("imported_dataset_references"));
        Assert.True(attributes.ContainsKey("formula_catalog_version"));
        Assert.True(attributes.ContainsKey("rate_catalog_version"));
        Assert.True(attributes.ContainsKey("tariff_catalog_version"));
        Assert.True(attributes.ContainsKey("penalty_catalog_version"));
        Assert.True(attributes.ContainsKey("policy_catalog_version"));
        Assert.True(attributes.ContainsKey("optimization_model_catalog_version"));
        Assert.True(attributes.ContainsKey("optimization_strategy_catalog_version"));
        Assert.True(attributes.ContainsKey("provider_catalog_version"));
        Assert.True(attributes.ContainsKey("effective_date_utc"));
        Assert.True(attributes.ContainsKey("configuration_version"));
    }

    [Fact]
    public void Execute_ShouldConsumeBusinessContextAndConfigurationAssets()
    {
        var catalog = new RecordingBusinessConfigurationCatalog();
        var service = CreateService(catalog);

        var result = service.Execute(CreateRequest());

        Assert.Equal("cfg-v1", result.BusinessContext.Metadata.ConfigurationVersion);

        Assert.Equal(13, catalog.ConsumedKeys.Count);
        Assert.Contains("subsidyoptimization.catalog.import-definition", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.formula", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.rate", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.tariff", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.penalty", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.policy", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.provider", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.optimization-model", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.optimization-strategy", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.language-resource", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.report-definition", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.notification-template", catalog.ConsumedKeys);
        Assert.Contains("subsidyoptimization.catalog.document-template", catalog.ConsumedKeys);
    }

    [Fact]
    public void Execute_ShouldProduceRecommendationsWithoutDecisionExecution()
    {
        var service = CreateService();

        var result = service.Execute(CreateRequest());

        Assert.All(result.RecommendationBundle.Recommendations, recommendation =>
        {
            Assert.Equal(RecommendationStatus.Proposed, recommendation.Status);
        });

        Assert.Null(result.RecommendationBundle.DecisionId);
    }

    [Fact]
    public void Execute_ShouldPreserveExpectedCalculationOutputs_AfterCalculationEngineMigration()
    {
        var service = CreateService();
        var request = CreateRequest();

        var result = service.Execute(request);
        var expectedEstimate = CalculateExpectedEstimate(request);
        var expectedForecast = CalculateExpectedForecast(expectedEstimate, request.RatedConsumptions);
        var expectedConfidence = CalculateExpectedConfidence(expectedEstimate, result.RankedScenarios, request.ConfidenceThreshold);

        AssertDecimalEqual(expectedEstimate.HistoricalAverageUnits, result.ConsumptionEstimate.HistoricalAverageUnits);
        AssertDecimalEqual(expectedEstimate.WeightedAverageUnits, result.ConsumptionEstimate.WeightedAverageUnits);
        AssertDecimalEqual(expectedEstimate.FailedMeterEstimateUnits, result.ConsumptionEstimate.FailedMeterEstimateUnits);
        AssertDecimalEqual(expectedEstimate.OccupancyAdjustedUnits, result.ConsumptionEstimate.OccupancyAdjustedUnits);
        AssertDecimalEqual(expectedEstimate.DataCompletenessRatio, result.ConsumptionEstimate.DataCompletenessRatio);

        AssertDecimalEqual(expectedForecast.ProjectedConsumptionUnits, result.Forecast.ProjectedConsumptionUnits);
        AssertDecimalEqual(expectedForecast.TrendFactor, result.Forecast.TrendFactor);
        AssertDecimalEqual(expectedForecast.ThresholdVarianceUnits, result.Forecast.ThresholdVarianceUnits);
        AssertDecimalEqual(expectedConfidence, result.Confidence.Value);
    }

    [Fact]
    public void Execute_ShouldInvokeCalculationEngineCapabilities_ForMigratedCalculations()
    {
        var runtime = new RecordingRuntimeInvoker();
        var service = CreateService(runtime: runtime);

        _ = service.Execute(CreateRequest());

        Assert.Contains("normalization.clamp", runtime.CapabilityIds);
        Assert.Contains("aggregation.mean", runtime.CapabilityIds);
        Assert.Contains("aggregation.weighted_mean", runtime.CapabilityIds);
        Assert.Contains("normalization.ratio", runtime.CapabilityIds);
        Assert.Contains("interpolation.weighted_blend", runtime.CapabilityIds);
        Assert.Contains("forecast.projection", runtime.CapabilityIds);
        Assert.Contains("statistics.spread", runtime.CapabilityIds);
        Assert.Contains("scoring.confidence", runtime.CapabilityIds);
        Assert.Contains("scoring.weighted_score", runtime.CapabilityIds);
        Assert.Contains("ranking.tie_break", runtime.CapabilityIds);
        Assert.All(runtime.Metadata, metadata => Assert.False(metadata.CapabilityId.IsDefault));
    }

    private static SubsidyMaximizerService CreateService(
        IBusinessConfigurationCatalog? catalog = null,
        SubsidyCalculationRuntimeInvoker? runtime = null)
    {
        var builder = new BusinessContextBuilder(new BusinessContextBuilderRegistry());
        var effectiveRuntime = runtime ?? new SubsidyCalculationRuntimeInvoker(new DelegatingCalculationRuntime());

        return new SubsidyMaximizerService(
            businessContextBuilder: builder,
            businessConfigurationCatalog: catalog ?? new RecordingBusinessConfigurationCatalog(),
            consumptionEstimator: new ConsumptionEstimator(effectiveRuntime),
            forecastEngine: new ForecastEngine(effectiveRuntime),
            scenarioGenerator: new ScenarioGenerator(),
            scenarioEvaluator: new ScenarioEvaluator(effectiveRuntime),
            confidenceScorer: new ConfidenceScorer(effectiveRuntime),
            recommendationGenerator: new RecommendationGenerator(
                new RecommendationExplanationBuilder(),
                new RecommendationEvidenceBuilder()),
            optimizationSessionBuilder: new OptimizationSessionBuilder());
    }

    private static SubsidyMaximizerRequest CreateRequest(
        IReadOnlyCollection<MeteringConsumptionHistoryContract>? consumptionHistory = null,
        decimal confidenceThreshold = 0.55m)
    {
        var history = consumptionHistory ??
            [
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 120m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 110m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 130m, DateTime.UtcNow)
            ];

        var rated =
            new[]
            {
                new RatedConsumptionContract(Guid.NewGuid(), history.First().MeterId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 118m, 35m, DateTime.UtcNow),
                new RatedConsumptionContract(Guid.NewGuid(), history.First().MeterId, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 121m, 39m, DateTime.UtcNow)
            };

        var imported =
            new[]
            {
                new ImportedDatasetReference("ds-consumption", "consumption", "import-export", "v3", DateTime.UtcNow),
                new ImportedDatasetReference("ds-occupancy", "occupancy", "import-export", "v1", DateTime.UtcNow)
            };

        return new SubsidyMaximizerRequest(
            ConsumptionHistory: history,
            RatedConsumptions: rated,
            ImportedDatasets: imported,
            EffectiveDateUtc: new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            ConfigurationVersion: "cfg-v1",
            OccupancyRate: 0.92m,
            ConfidenceThreshold: confidenceThreshold,
            TenantId: "tenant-1",
            PropertyId: "property-1",
            UserId: "user-1",
            PortfolioId: "portfolio-1",
            Language: "en-US",
            SecurityContext: "superuser",
            OptimizationModel: "deterministic-v1",
            OptimizationStrategy: "weighted-threshold");
    }

    private sealed class RecordingBusinessConfigurationCatalog : IBusinessConfigurationCatalog
    {
        public HashSet<string> ConsumedKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public BusinessConfigurationAsset<TPayload> Resolve<TPayload>(ConfigurationKey key, ConfigurationResolutionRequest request)
        {
            _ = request;
            ConsumedKeys.Add(key.Value);

            var metadata = new BusinessConfigurationMetadata(
                DefinitionId: key.Value,
                Name: key.Value,
                Version: 1,
                Status: BusinessConfigurationStatus.Active,
                Description: "test",
                EffectiveFromUtc: DateTime.UtcNow.AddDays(-1),
                EffectiveToUtc: null,
                CreatedBy: "test",
                ModifiedBy: "test",
                CreatedAtUtc: DateTime.UtcNow.AddDays(-1),
                ModifiedAtUtc: DateTime.UtcNow,
                AuditMetadata: new Dictionary<string, string>());

            object payload = JsonDocument.Parse("{}").RootElement;

            return new BusinessConfigurationAsset<TPayload>(
                metadata,
                (TPayload)payload);
        }
    }

    private sealed class RecordingRuntimeInvoker : SubsidyCalculationRuntimeInvoker
    {
        public List<string> CapabilityIds { get; } = [];

        public List<ICalculationExecutionMetadata> Metadata { get; } = [];

        public RecordingRuntimeInvoker()
            : base(new DelegatingCalculationRuntime())
        {
        }

        public override ICalculationResult Execute(
            string capabilityId,
            IReadOnlyDictionary<string, object?> input,
            DateTime effectiveDateUtc)
        {
            var result = base.Execute(capabilityId, input, effectiveDateUtc);
            CapabilityIds.Add(capabilityId);
            Metadata.Add(result.ExecutionMetadata);
            return result;
        }
    }

    private sealed class DelegatingCalculationRuntime : ICalculationRuntime
    {
        private readonly Func<ICalculationRuntime> _factory = CreateDefaultRuntime;

        public ICalculationResult Execute(CalculationRuntimeRequest request)
        {
            return _factory().Execute(request);
        }

        private static ICalculationRuntime CreateDefaultRuntime()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddCalculationEngine();

            using var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ICalculationRuntime>();
        }
    }

    private static SubsidyConsumptionEstimate CalculateExpectedEstimate(SubsidyMaximizerRequest request)
    {
        var boundedOccupancy = Math.Clamp(request.OccupancyRate, 0m, 1m);
        var orderedHistory = request.ConsumptionHistory.OrderByDescending(x => x.PeriodEnd).ToArray();

        if (orderedHistory.Length == 0)
        {
            var baseline = request.RatedConsumptions.Count == 0
                ? 0m
                : request.RatedConsumptions.Average(x => x.RatedUnits);

            return new SubsidyConsumptionEstimate(
                baseline,
                baseline,
                baseline,
                baseline * boundedOccupancy,
                0m);
        }

        var historicalAverage = orderedHistory.Average(x => x.TotalConsumptionUnits);
        decimal weightedTotal = 0m;
        decimal weightSum = 0m;

        for (var index = 0; index < orderedHistory.Length; index++)
        {
            var weight = orderedHistory.Length - index;
            weightedTotal += orderedHistory[index].TotalConsumptionUnits * weight;
            weightSum += weight;
        }

        var weightedAverage = weightSum == 0m ? 0m : weightedTotal / weightSum;
        var ratedAverage = request.RatedConsumptions.Count == 0
            ? weightedAverage
            : request.RatedConsumptions.Average(x => x.RatedUnits);

        var failedReadings = orderedHistory.Count(x => x.TotalConsumptionUnits <= 0m);
        var failedRatio = orderedHistory.Length == 0 ? 0m : (decimal)failedReadings / orderedHistory.Length;
        var failedMeterEstimate = (weightedAverage * (1m - failedRatio)) + (ratedAverage * failedRatio);
        var occupancyAdjusted = failedMeterEstimate * boundedOccupancy;
        var completeness = Math.Clamp(1m - failedRatio, 0m, 1m);

        return new SubsidyConsumptionEstimate(
            historicalAverage,
            weightedAverage,
            failedMeterEstimate,
            occupancyAdjusted,
            completeness);
    }

    private static SubsidyForecast CalculateExpectedForecast(
        SubsidyConsumptionEstimate estimate,
        IReadOnlyCollection<RatedConsumptionContract> ratedConsumptions)
    {
        var ratedAverage = ratedConsumptions.Count == 0
            ? estimate.WeightedAverageUnits
            : ratedConsumptions.Average(x => x.RatedUnits);

        var trendFactor = estimate.WeightedAverageUnits == 0m
            ? 1m
            : ratedAverage / estimate.WeightedAverageUnits;

        var projected = estimate.OccupancyAdjustedUnits * trendFactor;
        var thresholdVariance = projected - estimate.OccupancyAdjustedUnits;

        return new SubsidyForecast(projected, trendFactor, thresholdVariance);
    }

    private static decimal CalculateExpectedConfidence(
        SubsidyConsumptionEstimate estimate,
        IReadOnlyCollection<SubsidyOptimizationScenario> scenarios,
        decimal minimumThreshold)
    {
        var boundedThreshold = Math.Clamp(minimumThreshold, 0m, 1m);
        var spread = scenarios.Count <= 1
            ? 0m
            : scenarios.Max(x => x.ForecastConsumptionUnits) - scenarios.Min(x => x.ForecastConsumptionUnits);
        var spreadPenalty = Math.Clamp(spread / 100m, 0m, 0.4m);
        return Math.Clamp(estimate.DataCompletenessRatio - spreadPenalty, boundedThreshold, 0.99m);
    }

    private static void AssertDecimalEqual(decimal expected, decimal actual, int precision = 10)
    {
        Assert.Equal(decimal.Round(expected, precision), decimal.Round(actual, precision));
    }
}
