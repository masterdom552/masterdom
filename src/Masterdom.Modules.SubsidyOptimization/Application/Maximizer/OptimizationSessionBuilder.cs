using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class OptimizationSessionBuilder
{
    public OptimizationSession Build(
        BusinessContext businessContext,
        SubsidyMaximizerRequest request,
        IReadOnlyDictionary<string, string> configurationVersions,
        string optimizationModel,
        string optimizationStrategy)
    {
        ArgumentNullException.ThrowIfNull(businessContext);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(configurationVersions);

        var nowUtc = DateTime.UtcNow;

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["business_context_version"] = businessContext.Version.ToString(),
            ["business_context_configuration_version"] = businessContext.Metadata.ConfigurationVersion ?? string.Empty,
            ["imported_dataset_references"] = string.Join("|", request.ImportedDatasets.Select(x => $"{x.DatasetType}:{x.DatasetId}:{x.Version}")),
            ["formula_catalog_version"] = configurationVersions["formula_catalog"],
            ["rate_catalog_version"] = configurationVersions["rate_catalog"],
            ["tariff_catalog_version"] = configurationVersions["tariff_catalog"],
            ["penalty_catalog_version"] = configurationVersions["penalty_catalog"],
            ["policy_catalog_version"] = configurationVersions["policy_catalog"],
            ["optimization_model_catalog_version"] = configurationVersions["optimization_model_catalog"],
            ["optimization_strategy_catalog_version"] = configurationVersions["optimization_strategy_catalog"],
            ["provider_catalog_version"] = configurationVersions["provider_catalog"],
            ["effective_date_utc"] = request.EffectiveDateUtc.ToString("O"),
            ["configuration_version"] = request.ConfigurationVersion,
            ["optimization_model"] = optimizationModel,
            ["optimization_strategy"] = optimizationStrategy
        };

        var metadata = new OptimizationSessionMetadata(
            createdAtUtc: nowUtc,
            effectiveDateUtc: request.EffectiveDateUtc,
            contextVersion: businessContext.Version.ToString(),
            recommendationVersion: "subsidy-maximizer-v1",
            attributes: attributes);

        return OptimizationSession
            .Create(OptimizationSessionId.New(), metadata)
            .Start(nowUtc)
            .Complete(nowUtc);
    }
}
