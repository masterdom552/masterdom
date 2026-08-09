namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyOptimizationScenario(
    string ScenarioCode,
    string ScenarioName,
    decimal EstimatedConsumptionUnits,
    decimal ForecastConsumptionUnits,
    decimal ExpectedSubsidy,
    decimal ExpectedCost,
    decimal SanctionedLoadImpact,
    decimal ExpectedBenefit,
    decimal ExpectedRisk,
    decimal ThresholdDelta,
    decimal SubsidyPreservationScore,
    bool IsFeasible,
    string? InfeasibilityReason,
    decimal? TriggeredBoundary,
    string TradeOffSummary,
    decimal RankScore,
    IReadOnlyList<SubsidyMeterAllocation> MeterAllocations);

public sealed record SubsidyMeterAllocation(
    Guid MeterId,
    decimal BaselineUnits,
    decimal AllocatedUnits,
    decimal SanctionedLoad,
    decimal MovementUnits);
