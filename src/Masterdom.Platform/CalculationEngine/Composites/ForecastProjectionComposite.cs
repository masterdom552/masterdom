using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class ForecastProjectionCompositeInput
{
    public ForecastProjectionCompositeInput(
        decimal baselineConsumption,
        decimal currentObservedConsumption,
        decimal previousObservedConsumption,
        decimal threshold)
    {
        BaselineConsumption = baselineConsumption;
        CurrentObservedConsumption = currentObservedConsumption;
        PreviousObservedConsumption = previousObservedConsumption;
        Threshold = threshold;
    }

    public decimal BaselineConsumption { get; }

    public decimal CurrentObservedConsumption { get; }

    public decimal PreviousObservedConsumption { get; }

    public decimal Threshold { get; }
}

internal sealed class ForecastProjectionCompositeOutput
{
    public ForecastProjectionCompositeOutput(
        decimal trendFactor,
        decimal projectedConsumption,
        decimal thresholdVariance)
    {
        TrendFactor = trendFactor;
        ProjectedConsumption = projectedConsumption;
        ThresholdVariance = thresholdVariance;
    }

    public decimal TrendFactor { get; }

    public decimal ProjectedConsumption { get; }

    public decimal ThresholdVariance { get; }
}

internal interface IForecastProjectionCompositeCalculator
    : ICalculationCompositeCalculator<ForecastProjectionCompositeInput, ForecastProjectionCompositeOutput>
{
}

internal sealed class ForecastProjectionCompositeCalculator : IForecastProjectionCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public ForecastProjectionCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public ForecastProjectionCompositeOutput Calculate(ForecastProjectionCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        var trendFactorOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationRatio,
            context,
            CompositePrimitiveExecutor.Input(
                ("numerator", input.CurrentObservedConsumption),
                ("denominator", input.PreviousObservedConsumption)));

        var trendFactor = CompositePrimitiveExecutor.ReadDecimal(trendFactorOutput, "value");

        var projectedOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.ProjectionTrendFactor,
            context,
            CompositePrimitiveExecutor.Input(("baseline", input.BaselineConsumption), ("trend_factor", trendFactor)));

        var projectedConsumption = CompositePrimitiveExecutor.ReadDecimal(projectedOutput, "value");

        var varianceOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.ProjectionThresholdVariance,
            context,
            CompositePrimitiveExecutor.Input(("projected", projectedConsumption), ("threshold", input.Threshold)));

        var thresholdVariance = CompositePrimitiveExecutor.ReadDecimal(varianceOutput, "value");

        return new ForecastProjectionCompositeOutput(
            trendFactor: trendFactor,
            projectedConsumption: projectedConsumption,
            thresholdVariance: thresholdVariance);
    }
}
