namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyOptimizationScenario(
    string ScenarioCode,
    string ScenarioName,
    decimal EstimatedConsumptionUnits,
    decimal ForecastConsumptionUnits,
    decimal ExpectedBenefit,
    decimal ExpectedRisk,
    decimal ThresholdDelta,
    decimal SubsidyPreservationScore,
    string TradeOffSummary,
    decimal RankScore);
