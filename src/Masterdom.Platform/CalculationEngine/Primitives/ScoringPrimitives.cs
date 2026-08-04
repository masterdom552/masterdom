using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class ScoringWeightedScorePrimitive : CalculationPrimitiveOperationBase
{
    internal ScoringWeightedScorePrimitive()
        : base(PrimitiveCapabilityIds.ScoringWeightedScore, CalculationOperationCapabilityCategory.Scoring)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        var values = PrimitiveInput.GetDecimalArray(request.Input.Values, "values");
        var weights = PrimitiveInput.GetDecimalArray(request.Input.Values, "weights");
        return new WeightedAggregationInput(values, weights);
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (WeightedAggregationInput)input;
        PrimitiveInput.EnsureNonEmpty(model.Values, "values");
        PrimitiveInput.EnsureSameLength(model.Values, model.Weights, "values", "weights");

        decimal weightedSum = 0m;
        decimal weights = 0m;

        for (var i = 0; i < model.Values.Length; i++)
        {
            var weight = model.Weights[i];
            if (weight < 0m)
            {
                throw new CalculationOperationValidationException("Input 'weights' cannot contain negative values.");
            }

            weightedSum = checked(weightedSum + (model.Values[i] * weight));
            weights = checked(weights + weight);
        }

        if (weights == 0m)
        {
            throw new CalculationOperationValidationException("Input 'weights' must contain at least one positive value.");
        }

        return new Dictionary<string, object?>
        {
            ["value"] = weightedSum / weights
        };
    }
}

internal sealed class ScoringConfidencePrimitive : CalculationPrimitiveOperationBase
{
    internal ScoringConfidencePrimitive()
        : base(PrimitiveCapabilityIds.ScoringConfidence, CalculationOperationCapabilityCategory.Scoring)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        var values = request.Input.Values;

        return new ConfidenceInput(
            PrimitiveInput.GetDecimal(values, "quality"),
            PrimitiveInput.GetDecimal(values, "penalty"),
            values.ContainsKey("min") ? PrimitiveInput.GetDecimal(values, "min") : 0m,
            values.ContainsKey("max") ? PrimitiveInput.GetDecimal(values, "max") : 1m);
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (ConfidenceInput)input;

        if (model.Min > model.Max)
        {
            throw new CalculationOperationValidationException("Input 'min' must be less than or equal to 'max'.");
        }

        var raw = model.Quality - model.Penalty;
        var bounded = Math.Clamp(raw, model.Min, model.Max);

        return new Dictionary<string, object?>
        {
            ["value"] = bounded
        };
    }
}

internal sealed record ConfidenceInput(decimal Quality, decimal Penalty, decimal Min, decimal Max);
