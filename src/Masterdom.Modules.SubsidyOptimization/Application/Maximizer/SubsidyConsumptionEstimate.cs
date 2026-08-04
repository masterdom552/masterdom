namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyConsumptionEstimate(
    decimal HistoricalAverageUnits,
    decimal WeightedAverageUnits,
    decimal FailedMeterEstimateUnits,
    decimal OccupancyAdjustedUnits,
    decimal DataCompletenessRatio);
