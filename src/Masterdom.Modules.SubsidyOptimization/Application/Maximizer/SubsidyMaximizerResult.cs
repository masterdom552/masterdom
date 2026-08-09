using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Recommendation;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyMaximizerResult(
    BusinessContext BusinessContext,
    OptimizationSession OptimizationSession,
    RecommendationBundle RecommendationBundle,
    IReadOnlyList<SubsidyOptimizationScenario> RankedScenarios,
    SubsidyConsumptionEstimate ConsumptionEstimate,
    SubsidyForecast Forecast,
    RecommendationConfidence Confidence,
    IReadOnlyList<MeteringConsumptionHistoryContract> ParticipatingConsumptionHistory,
    IReadOnlyDictionary<string, string> ConsumedConfigurationVersions,
    ResolvedSubsidyOptimizerConfiguration GovernedConfiguration);
