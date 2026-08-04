using System.Text.Json;
using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class SubsidyMaximizerService : ISubsidyMaximizerService
{
    private static readonly IReadOnlyDictionary<string, string> ConfigurationAssetKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["import_definition_catalog"] = "subsidyoptimization.catalog.import-definition",
        ["formula_catalog"] = "subsidyoptimization.catalog.formula",
        ["rate_catalog"] = "subsidyoptimization.catalog.rate",
        ["tariff_catalog"] = "subsidyoptimization.catalog.tariff",
        ["penalty_catalog"] = "subsidyoptimization.catalog.penalty",
        ["policy_catalog"] = "subsidyoptimization.catalog.policy",
        ["provider_catalog"] = "subsidyoptimization.catalog.provider",
        ["optimization_model_catalog"] = "subsidyoptimization.catalog.optimization-model",
        ["optimization_strategy_catalog"] = "subsidyoptimization.catalog.optimization-strategy",
        ["language_resource_catalog"] = "subsidyoptimization.catalog.language-resource",
        ["report_definition_catalog"] = "subsidyoptimization.catalog.report-definition",
        ["notification_template_catalog"] = "subsidyoptimization.catalog.notification-template",
        ["document_template_catalog"] = "subsidyoptimization.catalog.document-template"
    };

    private readonly IBusinessContextBuilder _businessContextBuilder;
    private readonly IBusinessConfigurationCatalog _businessConfigurationCatalog;
    private readonly ConsumptionEstimator _consumptionEstimator;
    private readonly ForecastEngine _forecastEngine;
    private readonly ScenarioGenerator _scenarioGenerator;
    private readonly ScenarioEvaluator _scenarioEvaluator;
    private readonly ConfidenceScorer _confidenceScorer;
    private readonly RecommendationGenerator _recommendationGenerator;
    private readonly OptimizationSessionBuilder _optimizationSessionBuilder;

    public SubsidyMaximizerService(
        IBusinessContextBuilder businessContextBuilder,
        IBusinessConfigurationCatalog businessConfigurationCatalog,
        ConsumptionEstimator consumptionEstimator,
        ForecastEngine forecastEngine,
        ScenarioGenerator scenarioGenerator,
        ScenarioEvaluator scenarioEvaluator,
        ConfidenceScorer confidenceScorer,
        RecommendationGenerator recommendationGenerator,
        OptimizationSessionBuilder optimizationSessionBuilder)
    {
        _businessContextBuilder = businessContextBuilder ?? throw new ArgumentNullException(nameof(businessContextBuilder));
        _businessConfigurationCatalog = businessConfigurationCatalog ?? throw new ArgumentNullException(nameof(businessConfigurationCatalog));
        _consumptionEstimator = consumptionEstimator ?? throw new ArgumentNullException(nameof(consumptionEstimator));
        _forecastEngine = forecastEngine ?? throw new ArgumentNullException(nameof(forecastEngine));
        _scenarioGenerator = scenarioGenerator ?? throw new ArgumentNullException(nameof(scenarioGenerator));
        _scenarioEvaluator = scenarioEvaluator ?? throw new ArgumentNullException(nameof(scenarioEvaluator));
        _confidenceScorer = confidenceScorer ?? throw new ArgumentNullException(nameof(confidenceScorer));
        _recommendationGenerator = recommendationGenerator ?? throw new ArgumentNullException(nameof(recommendationGenerator));
        _optimizationSessionBuilder = optimizationSessionBuilder ?? throw new ArgumentNullException(nameof(optimizationSessionBuilder));
    }

    public SubsidyMaximizerResult Execute(SubsidyMaximizerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EffectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("EffectiveDateUtc must be UTC.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfigurationVersion))
        {
            throw new InvalidOperationException("ConfigurationVersion is required.");
        }

        var businessContext = BuildBusinessContext(request);
        var consumedConfigurationVersions = ResolveConfigurationVersions(request);

        var estimate = _consumptionEstimator.Estimate(
            request.ConsumptionHistory,
            request.RatedConsumptions,
            request.OccupancyRate,
            request.EffectiveDateUtc);

        var forecast = _forecastEngine.Forecast(estimate, request.RatedConsumptions, request.EffectiveDateUtc);
        var scenarios = _scenarioGenerator.Generate(estimate, forecast);
        var ranked = _scenarioEvaluator.RankScenarios(scenarios, request.EffectiveDateUtc);
        var confidence = _confidenceScorer.Score(estimate, ranked, request.ConfidenceThreshold, request.EffectiveDateUtc);

        var optimizationModel = string.IsNullOrWhiteSpace(request.OptimizationModel)
            ? "deterministic-v1"
            : request.OptimizationModel.Trim();
        var optimizationStrategy = string.IsNullOrWhiteSpace(request.OptimizationStrategy)
            ? "weighted-threshold"
            : request.OptimizationStrategy.Trim();
        var effectivePolicy = consumedConfigurationVersions["policy_catalog"];

        var recommendations = _recommendationGenerator.Generate(
            ranked,
            estimate,
            forecast,
            confidence,
            request.ImportedDatasets,
            consumedConfigurationVersions,
            request.EffectiveDateUtc,
            optimizationModel,
            optimizationStrategy,
            effectivePolicy);

        var bundle = RecommendationBundle
            .CreateDraft(
                RecommendationBundleId.New(),
                recommendations,
                createdAtUtc: DateTime.UtcNow,
                effectiveDateUtc: request.EffectiveDateUtc,
                version: "subsidy-maximizer-v1")
            .Open()
            .FinalizeBundle();

        var session = _optimizationSessionBuilder.Build(
            businessContext,
            request,
            consumedConfigurationVersions,
            optimizationModel,
            optimizationStrategy);

        return new SubsidyMaximizerResult(
            BusinessContext: businessContext,
            OptimizationSession: session,
            RecommendationBundle: bundle,
            RankedScenarios: ranked,
            ConsumptionEstimate: estimate,
            Forecast: forecast,
            Confidence: confidence,
            ConsumedConfigurationVersions: consumedConfigurationVersions);
    }

    private BusinessContext BuildBusinessContext(SubsidyMaximizerRequest request)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["subsidy_maximizer_v1"] = "true",
            ["imported_dataset_count"] = request.ImportedDatasets.Count.ToString(),
            ["occupancy_rate"] = request.OccupancyRate.ToString("F4")
        };

        var contextRequest = new BusinessContextRequest(
            effectiveDateUtc: request.EffectiveDateUtc,
            configurationVersion: request.ConfigurationVersion,
            language: request.Language,
            securityContext: request.SecurityContext,
            userId: request.UserId,
            portfolioId: request.PortfolioId,
            attributes: attributes);

        return _businessContextBuilder.Build(contextRequest).Context;
    }

    private Dictionary<string, string> ResolveConfigurationVersions(SubsidyMaximizerRequest request)
    {
        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var resolutionRequest = new ConfigurationResolutionRequest
        {
            ModuleId = "subsidyoptimization",
            TenantId = request.TenantId,
            PropertyId = request.PropertyId,
            AsOfUtc = request.EffectiveDateUtc
        };

        foreach (var pair in ConfigurationAssetKeys)
        {
            try
            {
                var asset = _businessConfigurationCatalog.Resolve<JsonElement>(
                    new ConfigurationKey(pair.Value),
                    resolutionRequest);

                resolved[pair.Key] = $"v{asset.Metadata.Version}";
            }
            catch
            {
                // Missing optional catalog data should not block deterministic recommendation generation.
                resolved[pair.Key] = "unresolved";
            }
        }

        return resolved;
    }
}
