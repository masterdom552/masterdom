namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyForecast(
    decimal ProjectedConsumptionUnits,
    decimal TrendFactor,
    decimal ThresholdVarianceUnits);
