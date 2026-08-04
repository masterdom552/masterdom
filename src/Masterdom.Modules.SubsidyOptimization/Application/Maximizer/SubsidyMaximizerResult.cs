using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Recommendation;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyMaximizerResult(
    BusinessContext BusinessContext,
    OptimizationSession OptimizationSession,
    RecommendationBundle RecommendationBundle,
    IReadOnlyList<SubsidyOptimizationScenario> RankedScenarios,
    SubsidyConsumptionEstimate ConsumptionEstimate,
    SubsidyForecast Forecast,
    RecommendationConfidence Confidence,
    IReadOnlyDictionary<string, string> ConsumedConfigurationVersions);
