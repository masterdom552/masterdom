using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class RankingOrderPrimitive : CalculationPrimitiveOperationBase
{
    internal RankingOrderPrimitive()
        : base(PrimitiveCapabilityIds.RankingOrder, CalculationOperationCapabilityCategory.Ranking)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new RankingOrderInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "values"),
            PrimitiveInput.GetBool(request.Input.Values, "descending", true));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (RankingOrderInput)input;
        PrimitiveInput.EnsureNonEmpty(model.Values, "values");

        var ordered = model.Descending
            ? model.Values.OrderByDescending(v => v).ToArray()
            : model.Values.OrderBy(v => v).ToArray();

        return new Dictionary<string, object?>
        {
            ["ordered_values"] = ordered
        };
    }
}

internal sealed class RankingTopNPrimitive : CalculationPrimitiveOperationBase
{
    internal RankingTopNPrimitive()
        : base(PrimitiveCapabilityIds.RankingTopN, CalculationOperationCapabilityCategory.Ranking)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new RankingTopNInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "ordered_values"),
            PrimitiveInput.GetInt(request.Input.Values, "count"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (RankingTopNInput)input;

        if (model.Count < 0)
        {
            throw new CalculationOperationValidationException("Input 'count' must be greater than or equal to zero.");
        }

        var selected = model.OrderedValues
            .Take(model.Count)
            .ToArray();

        return new Dictionary<string, object?>
        {
            ["selected_values"] = selected
        };
    }
}

internal sealed class RankingTieBreakPrimitive : CalculationPrimitiveOperationBase
{
    internal RankingTieBreakPrimitive()
        : base(PrimitiveCapabilityIds.RankingTieBreak, CalculationOperationCapabilityCategory.Ranking)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        var primary = PrimitiveInput.GetDecimalArray(request.Input.Values, "primary_scores");
        var secondary = PrimitiveInput.GetDecimalArray(request.Input.Values, "secondary_scores");

        return new RankingTieBreakInput(primary, secondary);
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (RankingTieBreakInput)input;
        PrimitiveInput.EnsureNonEmpty(model.PrimaryScores, "primary_scores");
        PrimitiveInput.EnsureSameLength(model.PrimaryScores, model.SecondaryScores, "primary_scores", "secondary_scores");

        var indices = Enumerable.Range(0, model.PrimaryScores.Length)
            .OrderByDescending(index => model.PrimaryScores[index])
            .ThenByDescending(index => model.SecondaryScores[index])
            .ThenBy(index => index)
            .ToArray();

        return new Dictionary<string, object?>
        {
            ["ordered_indices"] = indices
        };
    }
}

internal sealed record RankingOrderInput(decimal[] Values, bool Descending);

internal sealed record RankingTopNInput(decimal[] OrderedValues, int Count);

internal sealed record RankingTieBreakInput(decimal[] PrimaryScores, decimal[] SecondaryScores);
