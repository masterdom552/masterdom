using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class NormalizationClampPrimitive : CalculationPrimitiveOperationBase
{
    internal NormalizationClampPrimitive()
        : base(PrimitiveCapabilityIds.NormalizationClamp, CalculationOperationCapabilityCategory.Normalization)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new BoundedValueInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "value"),
            PrimitiveInput.GetDecimal(request.Input.Values, "min"),
            PrimitiveInput.GetDecimal(request.Input.Values, "max"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (BoundedValueInput)input;
        EnsureMinMax(model.Min, model.Max);

        return new Dictionary<string, object?>
        {
            ["value"] = Math.Clamp(model.Value, model.Min, model.Max)
        };
    }

    private static void EnsureMinMax(decimal min, decimal max)
    {
        if (min > max)
        {
            throw new CalculationOperationValidationException("Input 'min' must be less than or equal to 'max'.");
        }
    }
}

internal sealed class NormalizationRatioPrimitive : CalculationPrimitiveOperationBase
{
    internal NormalizationRatioPrimitive()
        : base(PrimitiveCapabilityIds.NormalizationRatio, CalculationOperationCapabilityCategory.Normalization)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new RatioInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "numerator"),
            PrimitiveInput.GetDecimal(request.Input.Values, "denominator"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (RatioInput)input;

        if (model.Denominator == 0m)
        {
            throw new CalculationOperationValidationException("Input 'denominator' cannot be zero.");
        }

        return new Dictionary<string, object?>
        {
            ["value"] = model.Numerator / model.Denominator
        };
    }
}

internal sealed class NormalizationBoundsGuardPrimitive : CalculationPrimitiveOperationBase
{
    internal NormalizationBoundsGuardPrimitive()
        : base(PrimitiveCapabilityIds.NormalizationBoundsGuard, CalculationOperationCapabilityCategory.Normalization)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new BoundedValueInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "value"),
            PrimitiveInput.GetDecimal(request.Input.Values, "min"),
            PrimitiveInput.GetDecimal(request.Input.Values, "max"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (BoundedValueInput)input;

        if (model.Min > model.Max)
        {
            throw new CalculationOperationValidationException("Input 'min' must be less than or equal to 'max'.");
        }

        return new Dictionary<string, object?>
        {
            ["is_valid"] = model.Value >= model.Min && model.Value <= model.Max,
            ["bounded_value"] = Math.Clamp(model.Value, model.Min, model.Max)
        };
    }
}

internal sealed record BoundedValueInput(decimal Value, decimal Min, decimal Max);

internal sealed record RatioInput(decimal Numerator, decimal Denominator);
