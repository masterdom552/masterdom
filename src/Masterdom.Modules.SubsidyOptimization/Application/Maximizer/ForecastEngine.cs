using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ForecastEngine
{
    private readonly SubsidyCalculationRuntimeInvoker _runtime;

    public ForecastEngine(SubsidyCalculationRuntimeInvoker runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public SubsidyForecast Forecast(
        SubsidyConsumptionEstimate estimate,
        IReadOnlyCollection<RatedConsumptionContract> ratedConsumptions,
        DateTime effectiveDateUtc)
    {
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(ratedConsumptions);

        var ratedAverage = ratedConsumptions.Count == 0
            ? estimate.WeightedAverageUnits
            : SubsidyCalculationRuntimeInvoker.ReadDecimal(
                _runtime.Execute(
                    "aggregation.mean",
                    new Dictionary<string, object?>
                    {
                        ["values"] = ratedConsumptions.Select(x => x.RatedUnits).ToArray()
                    },
                    effectiveDateUtc),
                "value");

        if (estimate.WeightedAverageUnits == 0m)
        {
            return new SubsidyForecast(
                ProjectedConsumptionUnits: estimate.OccupancyAdjustedUnits,
                TrendFactor: 1m,
                ThresholdVarianceUnits: 0m);
        }

        var forecast = _runtime.Execute(
            "forecast.projection",
            new Dictionary<string, object?>
            {
                ["baselineConsumption"] = estimate.OccupancyAdjustedUnits,
                ["currentObservedConsumption"] = ratedAverage,
                ["previousObservedConsumption"] = estimate.WeightedAverageUnits,
                ["threshold"] = estimate.OccupancyAdjustedUnits
            },
            effectiveDateUtc);

        return new SubsidyForecast(
            ProjectedConsumptionUnits: SubsidyCalculationRuntimeInvoker.ReadDecimal(forecast, "projectedConsumption"),
            TrendFactor: SubsidyCalculationRuntimeInvoker.ReadDecimal(forecast, "trendFactor"),
            ThresholdVarianceUnits: SubsidyCalculationRuntimeInvoker.ReadDecimal(forecast, "thresholdVariance"));
    }
}
