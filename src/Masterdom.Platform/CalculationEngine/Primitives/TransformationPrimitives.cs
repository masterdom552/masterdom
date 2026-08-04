using System.Globalization;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal sealed class TransformationCanonicalDatePrimitive : CalculationPrimitiveOperationBase
{
    internal TransformationCanonicalDatePrimitive()
        : base(
            PrimitiveCapabilityIds.TransformationCanonicalDate,
            CalculationOperationCapabilityCategory.Transformation,
            CalculationOperationCompatibilityStatus.Experimental)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new CanonicalValueInput(PrimitiveInput.GetAny(request.Input.Values, "value"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var raw = ((CanonicalValueInput)input).Value;

        var date = raw switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            string text => DateOnly.Parse(text, CultureInfo.InvariantCulture),
            _ => throw new CalculationOperationValidationException("Input 'value' is not a supported date value.")
        };

        return new Dictionary<string, object?>
        {
            ["canonical_date"] = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
    }
}

internal sealed class TransformationCanonicalNumberPrimitive : CalculationPrimitiveOperationBase
{
    internal TransformationCanonicalNumberPrimitive()
        : base(
            PrimitiveCapabilityIds.TransformationCanonicalNumber,
            CalculationOperationCapabilityCategory.Transformation,
            CalculationOperationCompatibilityStatus.Experimental)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new CanonicalValueInput(PrimitiveInput.GetAny(request.Input.Values, "value"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var number = PrimitiveInput.ToDecimal(((CanonicalValueInput)input).Value, "value");

        return new Dictionary<string, object?>
        {
            ["canonical_number"] = number.ToString("G29", CultureInfo.InvariantCulture),
            ["value"] = number
        };
    }
}

internal sealed class TransformationCanonicalBooleanPrimitive : CalculationPrimitiveOperationBase
{
    internal TransformationCanonicalBooleanPrimitive()
        : base(
            PrimitiveCapabilityIds.TransformationCanonicalBoolean,
            CalculationOperationCapabilityCategory.Transformation,
            CalculationOperationCompatibilityStatus.Experimental)
    {
    }

    protected override object ParseInput(ICalculationRequest request)
    {
        return new CanonicalValueInput(PrimitiveInput.GetAny(request.Input.Values, "value"));
    }

    protected override IReadOnlyDictionary<string, object?> ExecuteCore(object input)
    {
        var raw = ((CanonicalValueInput)input).Value;
        if (raw is null)
        {
            throw new CalculationOperationValidationException("Input 'value' cannot be null.");
        }

        var value = PrimitiveInput.ToBool(raw, "value");

        return new Dictionary<string, object?>
        {
            ["canonical_boolean"] = value.ToString().ToLowerInvariant(),
            ["value"] = value
        };
    }
}

internal sealed record CanonicalValueInput(object? Value);
