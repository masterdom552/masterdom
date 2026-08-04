using System.Collections.Immutable;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class ConsumptionEstimationCompositeInput
{
    public ConsumptionEstimationCompositeInput(
        IEnumerable<decimal> historicalValues,
        IEnumerable<decimal> historicalWeights,
        decimal blendWeight,
        decimal occupancyNumerator,
        decimal occupancyDenominator,
        decimal completenessObservedCount,
        decimal completenessExpectedCount,
        decimal clampMin,
        decimal clampMax)
    {
        HistoricalValues = CompositePrimitiveExecutor.ToImmutableDecimalArray(historicalValues, nameof(historicalValues));
        HistoricalWeights = CompositePrimitiveExecutor.ToImmutableDecimalArray(historicalWeights, nameof(historicalWeights));
        BlendWeight = blendWeight;
        OccupancyNumerator = occupancyNumerator;
        OccupancyDenominator = occupancyDenominator;
        CompletenessObservedCount = CompositePrimitiveExecutor.ToNonNegativeDecimal(completenessObservedCount, nameof(completenessObservedCount));
        CompletenessExpectedCount = CompositePrimitiveExecutor.ToNonNegativeDecimal(completenessExpectedCount, nameof(completenessExpectedCount));
        ClampMin = clampMin;
        ClampMax = clampMax;
    }

    public ImmutableArray<decimal> HistoricalValues { get; }

    public ImmutableArray<decimal> HistoricalWeights { get; }

    public decimal BlendWeight { get; }

    public decimal OccupancyNumerator { get; }

    public decimal OccupancyDenominator { get; }

    public decimal CompletenessObservedCount { get; }

    public decimal CompletenessExpectedCount { get; }

    public decimal ClampMin { get; }

    public decimal ClampMax { get; }
}

internal sealed class ConsumptionEstimationCompositeOutput
{
    public ConsumptionEstimationCompositeOutput(
        decimal estimatedBaseline,
        decimal failedMeterEstimate,
        decimal occupancyAdjustedEstimate,
        decimal dataCompletenessRatio)
    {
        EstimatedBaseline = estimatedBaseline;
        FailedMeterEstimate = failedMeterEstimate;
        OccupancyAdjustedEstimate = occupancyAdjustedEstimate;
        DataCompletenessRatio = dataCompletenessRatio;
    }

    public decimal EstimatedBaseline { get; }

    public decimal FailedMeterEstimate { get; }

    public decimal OccupancyAdjustedEstimate { get; }

    public decimal DataCompletenessRatio { get; }
}

internal interface IConsumptionEstimationCompositeCalculator
    : ICalculationCompositeCalculator<ConsumptionEstimationCompositeInput, ConsumptionEstimationCompositeOutput>
{
}

internal sealed class ConsumptionEstimationCompositeCalculator : IConsumptionEstimationCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public ConsumptionEstimationCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public ConsumptionEstimationCompositeOutput Calculate(ConsumptionEstimationCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        var meanOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.AggregationMean,
            context,
            CompositePrimitiveExecutor.Input(("values", input.HistoricalValues)));

        var weightedMeanOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.AggregationWeightedMean,
            context,
            CompositePrimitiveExecutor.Input(("values", input.HistoricalValues), ("weights", input.HistoricalWeights)));

        var mean = CompositePrimitiveExecutor.ReadDecimal(meanOutput, "value");
        var weightedMean = CompositePrimitiveExecutor.ReadDecimal(weightedMeanOutput, "value");

        var failedMeterEstimateOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.InterpolationWeightedBlend,
            context,
            CompositePrimitiveExecutor.Input(("left", mean), ("right", weightedMean), ("weight", input.BlendWeight)));

        var failedMeterEstimate = CompositePrimitiveExecutor.ReadDecimal(failedMeterEstimateOutput, "value");

        var occupancyRatioOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationRatio,
            context,
            CompositePrimitiveExecutor.Input(("numerator", input.OccupancyNumerator), ("denominator", input.OccupancyDenominator)));

        var occupancyRatio = CompositePrimitiveExecutor.ReadDecimal(occupancyRatioOutput, "value");
        var occupancyAdjustedRaw = failedMeterEstimate * occupancyRatio;

        var occupancyAdjustedOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationClamp,
            context,
            CompositePrimitiveExecutor.Input(("value", occupancyAdjustedRaw), ("min", input.ClampMin), ("max", input.ClampMax)));

        var occupancyAdjustedEstimate = CompositePrimitiveExecutor.ReadDecimal(occupancyAdjustedOutput, "value");

        var completenessRatioOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationRatio,
            context,
            CompositePrimitiveExecutor.Input(("numerator", input.CompletenessObservedCount), ("denominator", input.CompletenessExpectedCount)));

        var completenessRatioRaw = CompositePrimitiveExecutor.ReadDecimal(completenessRatioOutput, "value");

        var completenessClampOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationClamp,
            context,
            CompositePrimitiveExecutor.Input(("value", completenessRatioRaw), ("min", 0m), ("max", 1m)));

        var dataCompletenessRatio = CompositePrimitiveExecutor.ReadDecimal(completenessClampOutput, "value");

        return new ConsumptionEstimationCompositeOutput(
            estimatedBaseline: mean,
            failedMeterEstimate: failedMeterEstimate,
            occupancyAdjustedEstimate: occupancyAdjustedEstimate,
            dataCompletenessRatio: dataCompletenessRatio);
    }
}
