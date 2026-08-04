using System.Collections.Immutable;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class ScenarioRankingCompositeInput
{
    public ScenarioRankingCompositeInput(
        IEnumerable<decimal> primaryScores,
        IEnumerable<decimal> secondaryScores,
        int topCount)
    {
        PrimaryScores = CompositePrimitiveExecutor.ToImmutableDecimalArray(primaryScores, nameof(primaryScores));
        SecondaryScores = CompositePrimitiveExecutor.ToImmutableDecimalArray(secondaryScores, nameof(secondaryScores));
        TopCount = CompositePrimitiveExecutor.ToNonNegativeInt(topCount, nameof(topCount));
    }

    public ImmutableArray<decimal> PrimaryScores { get; }

    public ImmutableArray<decimal> SecondaryScores { get; }

    public int TopCount { get; }
}

internal sealed class ScenarioRankingCompositeOutput
{
    public ScenarioRankingCompositeOutput(IEnumerable<int> rankedScenarioCollection)
    {
        RankedScenarioCollection = rankedScenarioCollection.ToImmutableArray();
    }

    public ImmutableArray<int> RankedScenarioCollection { get; }
}

internal interface IScenarioRankingCompositeCalculator
    : ICalculationCompositeCalculator<ScenarioRankingCompositeInput, ScenarioRankingCompositeOutput>
{
}

internal sealed class ScenarioRankingCompositeCalculator : IScenarioRankingCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public ScenarioRankingCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public ScenarioRankingCompositeOutput Calculate(ScenarioRankingCompositeInput input, ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        _ = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.RankingOrder,
            context,
            CompositePrimitiveExecutor.Input(("values", input.PrimaryScores), ("descending", true)));

        var tieBreakOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.RankingTieBreak,
            context,
            CompositePrimitiveExecutor.Input(("primary_scores", input.PrimaryScores), ("secondary_scores", input.SecondaryScores)));

        var orderedIndices = CompositePrimitiveExecutor.ReadIntArray(tieBreakOutput, "ordered_indices");

        var orderedIndicesAsDecimals = orderedIndices
            .Select(index => (decimal)index)
            .ToImmutableArray();

        var topNOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.RankingTopN,
            context,
            CompositePrimitiveExecutor.Input(("ordered_values", orderedIndicesAsDecimals), ("count", input.TopCount)));

        var selectedIndices = CompositePrimitiveExecutor.ReadDecimalArray(topNOutput, "selected_values")
            .Select(value => decimal.ToInt32(value))
            .ToImmutableArray();

        return new ScenarioRankingCompositeOutput(selectedIndices);
    }
}
