using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ConsumptionEstimator
{
    private readonly SubsidyCalculationRuntimeInvoker _runtime;

    public ConsumptionEstimator(SubsidyCalculationRuntimeInvoker runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public SubsidyConsumptionEstimate Estimate(
        IReadOnlyCollection<MeteringConsumptionHistoryContract> history,
        IReadOnlyCollection<RatedConsumptionContract> ratedConsumptions,
        decimal occupancyRate,
        DateTime effectiveDateUtc)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(ratedConsumptions);

        var boundedOccupancy = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.clamp",
                new Dictionary<string, object?>
                {
                    ["value"] = occupancyRate,
                    ["min"] = 0m,
                    ["max"] = 1m
                },
                effectiveDateUtc),
            "value");

        var orderedHistory = history.OrderByDescending(x => x.PeriodEnd).ToArray();
        var ratedUnits = ratedConsumptions.Select(x => x.RatedUnits).ToArray();

        if (orderedHistory.Length == 0)
        {
            var baseline = ratedUnits.Length == 0
                ? 0m
                : SubsidyCalculationRuntimeInvoker.ReadDecimal(
                    _runtime.Execute(
                        "aggregation.mean",
                        new Dictionary<string, object?>
                        {
                            ["values"] = ratedUnits
                        },
                        effectiveDateUtc),
                    "value");

            return new SubsidyConsumptionEstimate(
                HistoricalAverageUnits: baseline,
                WeightedAverageUnits: baseline,
                FailedMeterEstimateUnits: baseline,
                OccupancyAdjustedUnits: baseline * boundedOccupancy,
                DataCompletenessRatio: 0m);
        }

        var historicalValues = orderedHistory.Select(x => x.TotalConsumptionUnits).ToArray();
        var weights = Enumerable.Range(0, orderedHistory.Length)
            .Select(index => (decimal)(orderedHistory.Length - index))
            .ToArray();

        var historicalAverage = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "aggregation.mean",
                new Dictionary<string, object?>
                {
                    ["values"] = historicalValues
                },
                effectiveDateUtc),
            "value");

        var weightedAverage = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "aggregation.weighted_mean",
                new Dictionary<string, object?>
                {
                    ["values"] = historicalValues,
                    ["weights"] = weights
                },
                effectiveDateUtc),
            "value");

        var ratedAverage = ratedUnits.Length == 0
            ? weightedAverage
            : SubsidyCalculationRuntimeInvoker.ReadDecimal(
                _runtime.Execute(
                    "aggregation.mean",
                    new Dictionary<string, object?>
                    {
                        ["values"] = ratedUnits
                    },
                    effectiveDateUtc),
                "value");

        var failedReadings = orderedHistory.Count(x => x.TotalConsumptionUnits <= 0m);
        var failedRatio = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.ratio",
                new Dictionary<string, object?>
                {
                    ["numerator"] = failedReadings,
                    ["denominator"] = orderedHistory.Length
                },
                effectiveDateUtc),
            "value");

        var failedMeterEstimate = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "interpolation.weighted_blend",
                new Dictionary<string, object?>
                {
                    ["left"] = weightedAverage,
                    ["right"] = ratedAverage,
                    ["weight"] = failedRatio
                },
                effectiveDateUtc),
            "value");

        var occupancyAdjusted = failedMeterEstimate * boundedOccupancy;
        var completeness = SubsidyCalculationRuntimeInvoker.ReadDecimal(
            _runtime.Execute(
                "normalization.clamp",
                new Dictionary<string, object?>
                {
                    ["value"] = 1m - failedRatio,
                    ["min"] = 0m,
                    ["max"] = 1m
                },
                effectiveDateUtc),
            "value");

        return new SubsidyConsumptionEstimate(
            HistoricalAverageUnits: historicalAverage,
            WeightedAverageUnits: weightedAverage,
            FailedMeterEstimateUnits: failedMeterEstimate,
            OccupancyAdjustedUnits: occupancyAdjusted,
            DataCompletenessRatio: completeness);
    }
}
