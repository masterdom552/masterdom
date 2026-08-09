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
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, DateTime.UtcNow, "residential", "Active", 120m),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 0m, DateTime.UtcNow, "residential", "Active", 120m),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 0m, DateTime.UtcNow, "residential", "Active", 120m)
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
    public void Execute_ShouldEvaluateConfiguredCliffsAndSanctionedLoadImpact()
    {
        var result = CreateService().Execute(CreateRequest());

        Assert.Contains(result.RankedScenarios, x => x.ForecastConsumptionUnits == 100m && x.TriggeredBoundary == 100m);
        Assert.Contains(result.RankedScenarios, x => x.ForecastConsumptionUnits == 100.01m);
        Assert.Contains(result.RankedScenarios, x => x.SanctionedLoadImpact > 0m && x.ExpectedCost > 0m);
        Assert.All(result.RankedScenarios, x => Assert.False(string.IsNullOrWhiteSpace(x.TradeOffSummary)));
    }

    [Fact]
    public void Execute_ShouldRejectMeterTypeExcludedByGovernedPolicy()
    {
        var request = CreateRequest(consumptionHistory:
        [
            new MeteringConsumptionHistoryContract(
                Guid.NewGuid(),
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 31),
                100m,
                DateTime.UtcNow,
                MeterType: "commercial",
                MeterStatus: "Active",
                SanctionedLoad: 120m)
        ]);

        var exception = Assert.Throws<InvalidOperationException>(() => CreateService().Execute(request));
        Assert.Contains("not eligible", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(InvalidSanctionedLoads))]
    public void Execute_ShouldRejectMissingOrNonpositiveSanctionedLoad(decimal? sanctionedLoad)
    {
        var request = CreateRequest(consumptionHistory:
        [
            new MeteringConsumptionHistoryContract(
                Guid.NewGuid(),
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 31),
                100m,
                DateTime.UtcNow,
                "residential",
                "Active",
                sanctionedLoad)
        ]);

        var exception = Assert.Throws<InvalidOperationException>(() => CreateService().Execute(request));

        Assert.Contains("positive sanctioned load", exception.Message, StringComparison.Ordinal);
    }

    public static TheoryData<decimal?> InvalidSanctionedLoads => new()
    {
        null,
        0m,
        -1m
    };

    [Fact]
    public void Execute_ShouldRejectRatingOnlyMeter()
    {
        var request = CreateRequest();
        request = request with
        {
            RatedConsumptions =
            [
                new RatedConsumptionContract(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 1, 31),
                    100m,
                    50m,
                    DateTime.UtcNow)
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => CreateService().Execute(request));

        Assert.Contains("does not correlate", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_ShouldExcludeInactiveMetersBeforeForecastAndAllocation()
    {
        var activeMeterId = Guid.NewGuid();
        var retiredMeterId = Guid.NewGuid();
        var history = new[]
        {
            new MeteringConsumptionHistoryContract(activeMeterId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, DateTime.UtcNow, "residential", "Active", 120m),
            new MeteringConsumptionHistoryContract(retiredMeterId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 10_000m, DateTime.UtcNow, "residential", "Retired", 120m)
        };
        var request = CreateRequest(history) with
        {
            RatedConsumptions =
            [
                new RatedConsumptionContract(Guid.NewGuid(), activeMeterId, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, 50m, DateTime.UtcNow)
            ]
        };

        var result = CreateService().Execute(request);

        Assert.Single(result.ParticipatingConsumptionHistory);
        Assert.Equal(activeMeterId, result.ParticipatingConsumptionHistory[0].MeterId);
        Assert.All(result.RankedScenarios.SelectMany(x => x.MeterAllocations), allocation =>
            Assert.Equal(activeMeterId, allocation.MeterId));
    }

    [Fact]
    public void Execute_ShouldRejectUnknownMeterStatus()
    {
        var input = CreateRequest().ConsumptionHistory.First();
        var request = CreateRequest([input with { MeterStatus = "Unknown" }]);

        var exception = Assert.Throws<InvalidOperationException>(() => CreateService().Execute(request));

        Assert.Contains("not recognized", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_ShouldPreservePositiveSanctionedLoadForEveryMeterAllocation()
    {
        var result = CreateService().Execute(CreateRequest());

        Assert.All(result.RankedScenarios, scenario =>
            Assert.All(scenario.MeterAllocations, allocation => Assert.True(allocation.SanctionedLoad > 0m)));
    }

    [Fact]
    public void Execute_ShouldRejectMalformedPolicyBeforeCalculation()
    {
        var runtime = new RecordingRuntimeInvoker();
        var catalog = new RecordingBusinessConfigurationCatalog
        {
            Policy = new SubsidyPolicyConfiguration(
                "invalid-policy",
                [new SubsidySlabConfiguration(200m, 50m, true), new SubsidySlabConfiguration(100m, 25m, true)],
                120m,
                1m,
                ["residential"])
        };

        Assert.Throws<InvalidOperationException>(() => CreateService(catalog, runtime).Execute(CreateRequest()));
        Assert.Empty(runtime.CapabilityIds);
    }

    [Fact]
    public void Execute_ShouldRejectMalformedModelBeforeCalculation()
    {
        var runtime = new RecordingRuntimeInvoker();
        var catalog = new RecordingBusinessConfigurationCatalog
        {
            Model = new OptimizationModelConfiguration("invalid-model", 1m, 1m, 1m, 1m, 0.01m, 5)
        };

        var exception = Assert.Throws<InvalidOperationException>(() => CreateService(catalog, runtime).Execute(CreateRequest()));

        Assert.Contains("mandatory subsidy cliff", exception.Message, StringComparison.Ordinal);
        Assert.Empty(runtime.CapabilityIds);
    }

    [Fact]
    public void Execute_ShouldRejectMalformedStrategyBeforeCalculation()
    {
        var runtime = new RecordingRuntimeInvoker();
        var catalog = new RecordingBusinessConfigurationCatalog
        {
            Strategy = new OptimizationStrategyConfiguration("invalid-strategy", [1m, -0.5m], true, false, 0m)
        };

        Assert.Throws<InvalidOperationException>(() => CreateService(catalog, runtime).Execute(CreateRequest()));
        Assert.Empty(runtime.CapabilityIds);
    }

    [Fact]
    public void Execute_ShouldBeDeterministicForIdenticalInputsAndConfiguration()
    {
        var service = CreateService();
        var request = CreateRequest();

        var first = service.Execute(request).RankedScenarios;
        var second = service.Execute(request).RankedScenarios;

        Assert.Equal(first.Count, second.Count);
        for (var index = 0; index < first.Count; index++)
        {
            Assert.Equal(first[index] with { MeterAllocations = [] }, second[index] with { MeterAllocations = [] });
            Assert.Equal<SubsidyMeterAllocation>(first[index].MeterAllocations, second[index].MeterAllocations);
        }
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
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 120m, DateTime.UtcNow, "residential", "Active", 120m),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 110m, DateTime.UtcNow, "residential", "Active", 120m),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 130m, DateTime.UtcNow, "residential", "Active", 120m)
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

        public SubsidyPolicyConfiguration? Policy { get; init; }

        public OptimizationModelConfiguration? Model { get; init; }

        public OptimizationStrategyConfiguration? Strategy { get; init; }

        public BusinessConfigurationAsset<TPayload> Resolve<TPayload>(ConfigurationKey key, ConfigurationResolutionRequest request)
        {
            _ = request;
            ConsumedKeys.Add(key.Value);
            var effectiveFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var metadata = new BusinessConfigurationMetadata(
                DefinitionId: key.Value,
                Name: key.Value,
                Version: 1,
                Status: BusinessConfigurationStatus.Active,
                Description: "test",
                EffectiveFromUtc: effectiveFromUtc,
                EffectiveToUtc: null,
                CreatedBy: "test",
                ModifiedBy: "test",
                CreatedAtUtc: effectiveFromUtc,
                ModifiedAtUtc: effectiveFromUtc,
                AuditMetadata: new Dictionary<string, string>());

            object payload = typeof(TPayload) switch
            {
                var type when type == typeof(SubsidyPolicyConfiguration) => Policy ?? new SubsidyPolicyConfiguration(
                    "delhi-residential",
                    [
                        new SubsidySlabConfiguration(100m, 100m, true),
                        new SubsidySlabConfiguration(200m, 50m, true),
                        new SubsidySlabConfiguration(decimal.MaxValue, 0m, false)
                    ],
                    SanctionedLoadLimit: 115m,
                    SanctionedLoadPenaltyPerUnit: 2m,
                    EligibleMeterTypes: ["residential"]),
                var type when type == typeof(OptimizationModelConfiguration) => Model ?? new OptimizationModelConfiguration(
                    "balanced-v1",
                    SubsidyWeight: 1m,
                    CostWeight: 1m,
                    LoadImpactWeight: 1m,
                    RiskWeight: 0.1m,
                    BoundaryTolerance: 0.01m,
                    MaximumScenarioCount: 20),
                var type when type == typeof(OptimizationStrategyConfiguration) => Strategy ?? new OptimizationStrategyConfiguration(
                    "bounded-cliff-search-v1",
                    ConsumptionFactors: [1m, 0.98m, 0.95m, 0.90m],
                    IncludeSubsidyBoundaries: true,
                    PermitCrossMeterMovement: false,
                    MaximumCrossMeterMovementFraction: 0m),
                _ => JsonDocument.Parse("{}").RootElement
            };

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
                0m,
                []);
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
            completeness,
            []);
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
