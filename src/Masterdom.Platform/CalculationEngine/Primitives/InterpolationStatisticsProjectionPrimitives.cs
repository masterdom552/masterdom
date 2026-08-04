using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class StatisticsSpreadPrimitive : CalculationPrimitiveOperationBase
{
    internal StatisticsSpreadPrimitive()
        : base(PrimitiveCapabilityIds.StatisticsSpread, CalculationOperationCapabilityCategory.Statistics)
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
            ["value"] = values.Max() - values.Min()
        };
    }
}

internal sealed class InterpolationWeightedBlendPrimitive : CalculationPrimitiveOperationBase
{
    internal InterpolationWeightedBlendPrimitive()
        : base(
            PrimitiveCapabilityIds.InterpolationWeightedBlend,
            CalculationOperationCapabilityCategory.Interpolation,
            CalculationOperationCompatibilityStatus.Experimental)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new BlendInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "left"),
            PrimitiveInput.GetDecimal(request.Input.Values, "right"),
            PrimitiveInput.GetDecimal(request.Input.Values, "weight"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (BlendInput)input;

        if (model.Weight < 0m || model.Weight > 1m)
        {
            throw new CalculationOperationValidationException("Input 'weight' must be within [0,1].");
        }

        return new Dictionary<string, object?>
        {
            ["value"] = (model.Left * (1m - model.Weight)) + (model.Right * model.Weight)
        };
    }
}

internal sealed class InterpolationReliabilityBlendPrimitive : CalculationPrimitiveOperationBase
{
    internal InterpolationReliabilityBlendPrimitive()
        : base(PrimitiveCapabilityIds.InterpolationReliabilityBlend, CalculationOperationCapabilityCategory.Interpolation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        var values = PrimitiveInput.GetDecimalArray(request.Input.Values, "values");
        var reliabilities = PrimitiveInput.GetDecimalArray(request.Input.Values, "reliabilities");

        return new ReliabilityBlendInput(values, reliabilities);
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (ReliabilityBlendInput)input;
        PrimitiveInput.EnsureNonEmpty(model.Values, "values");
        PrimitiveInput.EnsureSameLength(model.Values, model.Reliabilities, "values", "reliabilities");

        decimal weighted = 0m;
        decimal weights = 0m;

        for (var i = 0; i < model.Values.Length; i++)
        {
            var reliability = model.Reliabilities[i];
            if (reliability < 0m)
            {
                throw new CalculationOperationValidationException("Input 'reliabilities' cannot contain negative values.");
            }

            weighted = checked(weighted + (model.Values[i] * reliability));
            weights = checked(weights + reliability);
        }

        if (weights == 0m)
        {
            throw new CalculationOperationValidationException("Input 'reliabilities' must contain at least one positive value.");
        }

        return new Dictionary<string, object?>
        {
            ["value"] = weighted / weights
        };
    }
}

internal sealed class ProjectionTrendFactorPrimitive : CalculationPrimitiveOperationBase
{
    internal ProjectionTrendFactorPrimitive()
        : base(PrimitiveCapabilityIds.ProjectionTrendFactor, CalculationOperationCapabilityCategory.Projection)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new TrendProjectionInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "baseline"),
            PrimitiveInput.GetDecimal(request.Input.Values, "trend_factor"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (TrendProjectionInput)input;

        return new Dictionary<string, object?>
        {
            ["value"] = model.Baseline * model.TrendFactor
        };
    }
}

internal sealed class ProjectionThresholdVariancePrimitive : CalculationPrimitiveOperationBase
{
    internal ProjectionThresholdVariancePrimitive()
        : base(PrimitiveCapabilityIds.ProjectionThresholdVariance, CalculationOperationCapabilityCategory.Projection)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new ThresholdVarianceInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "projected"),
            PrimitiveInput.GetDecimal(request.Input.Values, "threshold"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (ThresholdVarianceInput)input;

        return new Dictionary<string, object?>
        {
            ["value"] = model.Projected - model.Threshold
        };
    }
}

internal sealed record BlendInput(decimal Left, decimal Right, decimal Weight);

internal sealed record ReliabilityBlendInput(decimal[] Values, decimal[] Reliabilities);

internal sealed record TrendProjectionInput(decimal Baseline, decimal TrendFactor);

internal sealed record ThresholdVarianceInput(decimal Projected, decimal Threshold);
