using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal sealed class CanonicalImportConversionCompositeInput
{
    public CanonicalImportConversionCompositeInput(
        object? rawDate,
        object? rawNumber,
        object? rawBoolean,
        decimal numberRangeMin,
        decimal numberRangeMax,
        bool inclusiveMin,
        bool inclusiveMax)
    {
        RawDate = rawDate;
        RawNumber = rawNumber;
        RawBoolean = rawBoolean;
        NumberRangeMin = numberRangeMin;
        NumberRangeMax = numberRangeMax;
        InclusiveMin = inclusiveMin;
        InclusiveMax = inclusiveMax;
    }

    public object? RawDate { get; }

    public object? RawNumber { get; }

    public object? RawBoolean { get; }

    public decimal NumberRangeMin { get; }

    public decimal NumberRangeMax { get; }

    public bool InclusiveMin { get; }

    public bool InclusiveMax { get; }
}

internal sealed class CanonicalImportConversionCompositeOutput
{
    public CanonicalImportConversionCompositeOutput(
        string canonicalDate,
        string canonicalNumber,
        string canonicalBoolean,
        bool isCanonicalNumberInRange)
    {
        CanonicalDate = canonicalDate;
        CanonicalNumber = canonicalNumber;
        CanonicalBoolean = canonicalBoolean;
        IsCanonicalNumberInRange = isCanonicalNumberInRange;
    }

    public string CanonicalDate { get; }

    public string CanonicalNumber { get; }

    public string CanonicalBoolean { get; }

    public bool IsCanonicalNumberInRange { get; }
}

internal interface ICanonicalImportConversionCompositeCalculator
    : ICalculationCompositeCalculator<CanonicalImportConversionCompositeInput, CanonicalImportConversionCompositeOutput>
{
}

internal sealed class CanonicalImportConversionCompositeCalculator : ICanonicalImportConversionCompositeCalculator
{
    private readonly CompositePrimitiveExecutor _primitiveExecutor;

    public CanonicalImportConversionCompositeCalculator(CompositePrimitiveExecutor? primitiveExecutor = null)
    {
        _primitiveExecutor = primitiveExecutor ?? new CompositePrimitiveExecutor();
    }

    public CanonicalImportConversionCompositeOutput Calculate(
        CanonicalImportConversionCompositeInput input,
        ICalculationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);

        var canonicalDateOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.TransformationCanonicalDate,
            context,
            CompositePrimitiveExecutor.Input(("value", input.RawDate)));

        var canonicalNumberOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.TransformationCanonicalNumber,
            context,
            CompositePrimitiveExecutor.Input(("value", input.RawNumber)));

        var canonicalBooleanOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.TransformationCanonicalBoolean,
            context,
            CompositePrimitiveExecutor.Input(("value", input.RawBoolean)));

        var canonicalNumberValue = CompositePrimitiveExecutor.ReadDecimal(canonicalNumberOutput, "value");

        var validationOutput = _primitiveExecutor.ExecutePrimitive(
            PrimitiveCapabilityIds.ValidationRange,
            context,
            CompositePrimitiveExecutor.Input(
                ("value", canonicalNumberValue),
                ("min", input.NumberRangeMin),
                ("max", input.NumberRangeMax),
                ("inclusive_min", input.InclusiveMin),
                ("inclusive_max", input.InclusiveMax)));

        var canonicalDate = CompositePrimitiveExecutor.ReadString(canonicalDateOutput, "canonical_date");
        var canonicalNumber = CompositePrimitiveExecutor.ReadString(canonicalNumberOutput, "canonical_number");
        var canonicalBoolean = CompositePrimitiveExecutor.ReadString(canonicalBooleanOutput, "canonical_boolean");
        var inRange = CompositePrimitiveExecutor.ReadBoolean(validationOutput, "is_valid");

        return new CanonicalImportConversionCompositeOutput(
            canonicalDate,
            canonicalNumber,
            canonicalBoolean,
            inRange);
    }
}
