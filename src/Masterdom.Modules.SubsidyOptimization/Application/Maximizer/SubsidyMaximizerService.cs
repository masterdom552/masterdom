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

        var governedConfiguration = ResolveGovernedConfiguration(request);
        SubsidyOptimizerConfigurationValidator.Validate(governedConfiguration, request.EffectiveDateUtc);

        var invalidStatus = request.ConsumptionHistory.FirstOrDefault(x =>
            !string.Equals(x.MeterStatus, "Installed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(x.MeterStatus, "Active", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(x.MeterStatus, "Retired", StringComparison.OrdinalIgnoreCase));
        if (invalidStatus is not null)
        {
            throw new InvalidOperationException($"Meter status '{invalidStatus.MeterStatus}' is not recognized for meter '{invalidStatus.MeterId}'.");
        }

        var activeHistory = request.ConsumptionHistory
            .Where(x => string.Equals(x.MeterStatus, "Active", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (activeHistory.Length == 0)
        {
            throw new InvalidOperationException("At least one active meter consumption input is required.");
        }

        var activeMeterIds = activeHistory.Select(x => x.MeterId).ToHashSet();
        var uncorrelatedRating = request.RatedConsumptions.FirstOrDefault(x => !activeMeterIds.Contains(x.MeterId));
        if (uncorrelatedRating is not null)
        {
            throw new InvalidOperationException($"Rated consumption for meter '{uncorrelatedRating.MeterId}' does not correlate to an active participating meter.");
        }

        var invalidLoad = activeHistory.FirstOrDefault(x => x.SanctionedLoad is null or <= 0m);
        if (invalidLoad is not null)
        {
            throw new InvalidOperationException($"A positive sanctioned load is required for meter '{invalidLoad.MeterId}'.");
        }

        var inconsistentMeter = activeHistory
            .GroupBy(x => x.MeterId)
            .FirstOrDefault(group => group.Select(x => x.SanctionedLoad).Distinct().Count() != 1
                || group.Select(x => x.MeterType).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 1);
        if (inconsistentMeter is not null)
        {
            throw new InvalidOperationException($"Meter type and sanctioned load must be consistent for meter '{inconsistentMeter.Key}'.");
        }

        var ineligibleMeter = activeHistory.FirstOrDefault(x =>
            !governedConfiguration.Policy.EligibleMeterTypes.Contains(x.MeterType, StringComparer.OrdinalIgnoreCase));
        if (ineligibleMeter is not null)
        {
            throw new InvalidOperationException($"Meter type '{ineligibleMeter.MeterType}' is not eligible under the governed subsidy policy.");
        }

        var businessContext = BuildBusinessContext(request);
        var consumedConfigurationVersions = ResolveConfigurationVersions(request, governedConfiguration.Versions);

        var estimate = _consumptionEstimator.Estimate(
            activeHistory,
            request.RatedConsumptions,
            request.OccupancyRate,
            request.EffectiveDateUtc);

        var forecast = _forecastEngine.Forecast(estimate, request.RatedConsumptions, request.EffectiveDateUtc);
        var scenarios = _scenarioGenerator.Generate(
            estimate,
            forecast,
            governedConfiguration.Policy,
            governedConfiguration.Strategy,
            governedConfiguration.Model);
        var ranked = _scenarioEvaluator.RankScenarios(
            scenarios,
            governedConfiguration.Policy,
            governedConfiguration.Model,
            governedConfiguration.Strategy,
            request.RatedConsumptions,
            request.EffectiveDateUtc);
        var confidence = _confidenceScorer.Score(estimate, ranked, request.ConfidenceThreshold, request.EffectiveDateUtc);

        var optimizationModel = governedConfiguration.Model.ModelCode;
        var optimizationStrategy = governedConfiguration.Strategy.StrategyCode;
        var effectivePolicyVersion = consumedConfigurationVersions["policy_catalog"];

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
            effectivePolicyVersion);

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
            ParticipatingConsumptionHistory: activeHistory,
            ConsumedConfigurationVersions: consumedConfigurationVersions,
            GovernedConfiguration: governedConfiguration);
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

    private ResolvedSubsidyOptimizerConfiguration ResolveGovernedConfiguration(SubsidyMaximizerRequest request)
    {
        var resolutionRequest = CreateResolutionRequest(request);
        var policy = _businessConfigurationCatalog.Resolve<SubsidyPolicyConfiguration>(
            new ConfigurationKey(ConfigurationAssetKeys["policy_catalog"]),
            resolutionRequest);
        var model = _businessConfigurationCatalog.Resolve<OptimizationModelConfiguration>(
            new ConfigurationKey(ConfigurationAssetKeys["optimization_model_catalog"]),
            resolutionRequest);
        var strategy = _businessConfigurationCatalog.Resolve<OptimizationStrategyConfiguration>(
            new ConfigurationKey(ConfigurationAssetKeys["optimization_strategy_catalog"]),
            resolutionRequest);

        return new ResolvedSubsidyOptimizerConfiguration(
            policy.Payload,
            model.Payload,
            strategy.Payload,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["policy_catalog"] = $"v{policy.Metadata.Version}",
                ["optimization_model_catalog"] = $"v{model.Metadata.Version}",
                ["optimization_strategy_catalog"] = $"v{strategy.Metadata.Version}"
            },
            CreateIdentity(ConfigurationAssetKeys["policy_catalog"], policy.Metadata, request),
            CreateIdentity(ConfigurationAssetKeys["optimization_model_catalog"], model.Metadata, request),
            CreateIdentity(ConfigurationAssetKeys["optimization_strategy_catalog"], strategy.Metadata, request));
    }

    private static ResolvedConfigurationIdentity CreateIdentity(
        string configurationKey,
        BusinessConfigurationMetadata metadata,
        SubsidyMaximizerRequest request)
    {
        return new ResolvedConfigurationIdentity(
            configurationKey,
            metadata.DefinitionId,
            metadata.Version,
            metadata.EffectiveFromUtc,
            metadata.EffectiveToUtc,
            request.TenantId,
            request.PropertyId);
    }

    private Dictionary<string, string> ResolveConfigurationVersions(
        SubsidyMaximizerRequest request,
        IReadOnlyDictionary<string, string> governedVersions)
    {
        var resolved = new Dictionary<string, string>(governedVersions, StringComparer.OrdinalIgnoreCase);
        var resolutionRequest = CreateResolutionRequest(request);

        foreach (var pair in ConfigurationAssetKeys.Where(x => !governedVersions.ContainsKey(x.Key)))
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

    private static ConfigurationResolutionRequest CreateResolutionRequest(SubsidyMaximizerRequest request)
    {
        return new ConfigurationResolutionRequest
        {
            ModuleId = "subsidyoptimization",
            TenantId = request.TenantId,
            PropertyId = request.PropertyId,
            AsOfUtc = request.EffectiveDateUtc
        };
    }
}
