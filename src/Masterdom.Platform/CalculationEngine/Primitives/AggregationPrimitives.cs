using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class AggregationSumPrimitive : CalculationPrimitiveOperationBase
{
    internal AggregationSumPrimitive()
        : base(PrimitiveCapabilityIds.AggregationSum, CalculationOperationCapabilityCategory.Aggregation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new AggregationValuesInput(PrimitiveInput.GetDecimalArray(request.Input.Values, "values"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var values = ((AggregationValuesInput)input).Values;
        PrimitiveInput.EnsureNonEmpty(values, "values");

        decimal sum = 0m;
        foreach (var value in values)
        {
            sum = checked(sum + value);
        }

        return new Dictionary<string, object?>
        {
            ["value"] = sum
        };
    }
}

internal sealed class AggregationMeanPrimitive : CalculationPrimitiveOperationBase
{
    internal AggregationMeanPrimitive()
        : base(PrimitiveCapabilityIds.AggregationMean, CalculationOperationCapabilityCategory.Aggregation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new AggregationValuesInput(PrimitiveInput.GetDecimalArray(request.Input.Values, "values"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var values = ((AggregationValuesInput)input).Values;
        PrimitiveInput.EnsureNonEmpty(values, "values");

        decimal sum = 0m;
        foreach (var value in values)
        {
            sum = checked(sum + value);
        }

        return new Dictionary<string, object?>
        {
            ["value"] = sum / values.Length
        };
    }
}

internal sealed class AggregationWeightedMeanPrimitive : CalculationPrimitiveOperationBase
{
    internal AggregationWeightedMeanPrimitive()
        : base(
            PrimitiveCapabilityIds.AggregationWeightedMean,
            CalculationOperationCapabilityCategory.Aggregation,
            CalculationOperationCompatibilityStatus.Deprecated)
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
        decimal weightSum = 0m;

        for (var i = 0; i < model.Values.Length; i++)
        {
            if (model.Weights[i] < 0m)
            {
                throw new CalculationOperationValidationException("Input 'weights' cannot contain negative values.");
            }

            weightedSum = checked(weightedSum + (model.Values[i] * model.Weights[i]));
            weightSum = checked(weightSum + model.Weights[i]);
        }

        if (weightSum == 0m)
        {
            throw new CalculationOperationValidationException("Input 'weights' must contain at least one positive weight.");
        }

        return new Dictionary<string, object?>
        {
            ["value"] = weightedSum / weightSum
        };
    }
}

internal sealed class AggregationMinimumPrimitive : CalculationPrimitiveOperationBase
{
    internal AggregationMinimumPrimitive()
        : base(PrimitiveCapabilityIds.AggregationMin, CalculationOperationCapabilityCategory.Aggregation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new AggregationValuesInput(PrimitiveInput.GetDecimalArray(request.Input.Values, "values"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var values = ((AggregationValuesInput)input).Values;
        PrimitiveInput.EnsureNonEmpty(values, "values");

        return new Dictionary<string, object?>
        {
            ["value"] = values.Min()
        };
    }
}

internal sealed class AggregationMaximumPrimitive : CalculationPrimitiveOperationBase
{
    internal AggregationMaximumPrimitive()
        : base(PrimitiveCapabilityIds.AggregationMax, CalculationOperationCapabilityCategory.Aggregation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new AggregationValuesInput(PrimitiveInput.GetDecimalArray(request.Input.Values, "values"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var values = ((AggregationValuesInput)input).Values;
        PrimitiveInput.EnsureNonEmpty(values, "values");

        return new Dictionary<string, object?>
        {
            ["value"] = values.Max()
        };
    }
}

internal sealed record AggregationValuesInput(decimal[] Values);

internal sealed record WeightedAggregationInput(decimal[] Values, decimal[] Weights);
