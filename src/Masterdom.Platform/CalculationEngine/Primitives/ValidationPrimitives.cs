using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class ValidationThresholdPrimitive : CalculationPrimitiveOperationBase
{
    internal ValidationThresholdPrimitive()
        : base(PrimitiveCapabilityIds.ValidationThreshold, CalculationOperationCapabilityCategory.Validation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new ThresholdValidationInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "value"),
            PrimitiveInput.GetDecimal(request.Input.Values, "threshold"),
            PrimitiveInput.GetString(request.Input.Values, "operator"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (ThresholdValidationInput)input;
        var op = model.Operator.Trim().ToLowerInvariant();

        var isValid = op switch
        {
            "lt" => model.Value < model.Threshold,
            "lte" => model.Value <= model.Threshold,
            "gt" => model.Value > model.Threshold,
            "gte" => model.Value >= model.Threshold,
            "eq" => model.Value == model.Threshold,
            "neq" => model.Value != model.Threshold,
            _ => throw new CalculationOperationValidationException("Input 'operator' must be one of: lt, lte, gt, gte, eq, neq.")
        };

        return new Dictionary<string, object?>
        {
            ["is_valid"] = isValid
        };
    }
}

internal sealed class ValidationRangePrimitive : CalculationPrimitiveOperationBase
{
    internal ValidationRangePrimitive()
        : base(PrimitiveCapabilityIds.ValidationRange, CalculationOperationCapabilityCategory.Validation)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new RangeValidationInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "value"),
            PrimitiveInput.GetDecimal(request.Input.Values, "min"),
            PrimitiveInput.GetDecimal(request.Input.Values, "max"),
            PrimitiveInput.GetBool(request.Input.Values, "inclusive_min", true),
            PrimitiveInput.GetBool(request.Input.Values, "inclusive_max", true));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var model = (RangeValidationInput)input;

        if (model.Min > model.Max)
        {
            throw new CalculationOperationValidationException("Input 'min' must be less than or equal to 'max'.");
        }

        var minPass = model.InclusiveMin ? model.Value >= model.Min : model.Value > model.Min;
        var maxPass = model.InclusiveMax ? model.Value <= model.Max : model.Value < model.Max;

        return new Dictionary<string, object?>
        {
            ["is_valid"] = minPass && maxPass
        };
    }
}

internal sealed record ThresholdValidationInput(decimal Value, decimal Threshold, string Operator);

internal sealed record RangeValidationInput(decimal Value, decimal Min, decimal Max, bool InclusiveMin, bool InclusiveMax);
