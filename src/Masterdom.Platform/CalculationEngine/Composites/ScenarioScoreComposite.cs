using System.Collections.Immutable;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class ScenarioScoreCompositeInput
{
    public ScenarioScoreCompositeInput(
        IEnumerable<decimal> componentValues,
        IEnumerable<decimal> componentWeights,
        decimal clampMin,
        decimal clampMax)
    {
        ComponentValues = CompositePrimitiveExecutor.ToImmutableDecimalArray(componentValues, nameof(componentValues));
        ComponentWeights = CompositePrimitiveExecutor.ToImmutableDecimalArray(componentWeights, nameof(componentWeights));
        ClampMin = clampMin;
        ClampMax = clampMax;
    }

    public ImmutableArray<decimal> ComponentValues { get; }

    public ImmutableArray<decimal> ComponentWeights { get; }

    public decimal ClampMin { get; }

    public decimal ClampMax { get; }
}

internal sealed class ScenarioScoreCompositeOutput
{
    public ScenarioScoreCompositeOutput(decimal compositeScenarioScore)
    {
        CompositeScenarioScore = compositeScenarioScore;
    }

    public decimal CompositeScenarioScore { get; }
}

internal interface IScenarioScoreCompositeCalculator
    : ICalculationCompositeCalculator<ScenarioScoreCompositeInput, ScenarioScoreCompositeOutput>
{
}

internal sealed class ScenarioScoreCompositeCalculator : IScenarioScoreCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public ScenarioScoreCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public ScenarioScoreCompositeOutput Calculate(ScenarioScoreCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        var weightedOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.ScoringWeightedScore,
            context,
            CompositePrimitiveExecutor.Input(("values", input.ComponentValues), ("weights", input.ComponentWeights)));

        var weightedScore = CompositePrimitiveExecutor.ReadDecimal(weightedOutput, "value");

        var clampedOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.NormalizationClamp,
            context,
            CompositePrimitiveExecutor.Input(("value", weightedScore), ("min", input.ClampMin), ("max", input.ClampMax)));

        var clamped = CompositePrimitiveExecutor.ReadDecimal(clampedOutput, "value");

        return new ScenarioScoreCompositeOutput(clamped);
    }
}
