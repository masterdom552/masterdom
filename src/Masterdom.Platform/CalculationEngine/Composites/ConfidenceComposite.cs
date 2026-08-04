using System.Collections.Immutable;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class ConfidenceCompositeInput
{
    public ConfidenceCompositeInput(
        IEnumerable<decimal> observedValues,
        decimal spreadUpperBound,
        decimal minConfidence,
        decimal maxConfidence)
    {
        ObservedValues = CompositePrimitiveExecutor.ToImmutableDecimalArray(observedValues, nameof(observedValues));
        SpreadUpperBound = spreadUpperBound;
        MinConfidence = minConfidence;
        MaxConfidence = maxConfidence;
    }

    public ImmutableArray<decimal> ObservedValues { get; }

    public decimal SpreadUpperBound { get; }

    public decimal MinConfidence { get; }

    public decimal MaxConfidence { get; }
}

internal sealed class ConfidenceCompositeOutput
{
    public ConfidenceCompositeOutput(decimal confidenceScore)
    {
        ConfidenceScore = confidenceScore;
    }

    public decimal ConfidenceScore { get; }
}

internal interface IConfidenceCompositeCalculator
    : ICalculationCompositeCalculator<ConfidenceCompositeInput, ConfidenceCompositeOutput>
{
}

internal sealed class ConfidenceCompositeCalculator : IConfidenceCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public ConfidenceCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public ConfidenceCompositeOutput Calculate(ConfidenceCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        if (input.SpreadUpperBound <= 0m)
        {
            throw new CalculationOperationValidationException("Input 'spreadUpperBound' must be greater than zero.");
        }

        var spreadOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.StatisticsSpread,
            context,
            CompositePrimitiveExecutor.Input(("values", input.ObservedValues)));

        var spread = CompositePrimitiveExecutor.ReadDecimal(spreadOutput, "value");
        var rawPenalty = spread / input.SpreadUpperBound;

        var penaltyClampOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationClamp,
            context,
            CompositePrimitiveExecutor.Input(("value", rawPenalty), ("min", input.MinConfidence), ("max", input.MaxConfidence)));

        var penalty = CompositePrimitiveExecutor.ReadDecimal(penaltyClampOutput, "value");

        var qualityClampOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationClamp,
            context,
            CompositePrimitiveExecutor.Input(("value", 1m - penalty), ("min", input.MinConfidence), ("max", input.MaxConfidence)));

        var quality = CompositePrimitiveExecutor.ReadDecimal(qualityClampOutput, "value");

        var confidenceOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.ScoringConfidence,
            context,
            CompositePrimitiveExecutor.Input(
                ("quality", quality),
                ("penalty", penalty),
                ("min", input.MinConfidence),
                ("max", input.MaxConfidence)));

        var confidenceScore = CompositePrimitiveExecutor.ReadDecimal(confidenceOutput, "value");

        return new ConfidenceCompositeOutput(confidenceScore);
    }
}
