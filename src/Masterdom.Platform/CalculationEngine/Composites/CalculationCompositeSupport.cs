using System.Collections.Immutable;
using System.Globalization;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Execution;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Composites;

internal static class CompositeCapabilityIds
{
    internal const string ConsumptionEstimation = "estimation.consumption";
    internal const string ForecastProjection = "forecast.projection";
    internal const string Confidence = "scoring.confidence_composite";
    internal const string ScenarioScore = "scoring.scenario";
    internal const string ScenarioRanking = "ranking.scenario";
    internal const string CanonicalImportConversion = "transformation.import_canonical";
    internal const string Pagination = "validation.pagination";
}

internal interface ICalculationCompositeCalculator<in TInput, out TOutput>
{
    TOutput Calculate(TInput input, ICalculationContext context);
}

internal sealed class CompositePrimitiveExecutor
{
    private readonly ICalculationEngine _engine;
    private readonly ICalculationOperationRegistry _metadataRegistry;

    internal CompositePrimitiveExecutor(
        ICalculationEngine? engine = null,
        ICalculationOperationRegistry? metadataRegistry = null)
    {
        _engine = engine ?? CalculationEngineFactory.CreateDefault();
        _metadataRegistry = metadataRegistry ?? new CalculationOperationRegistry();
    }

    internal IReadOnlyDictionary<string, object?> ExecutePrimitive(
        string capabilityId,
        ICalculationContext context,
        IReadOnlyDictionary<string, object?> input)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new ArgumentException("CapabilityId is required.", nameof(capabilityId));
        }

        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        var descriptor = _metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create(capabilityId));
        var request = new CalculationRequest(
            descriptor.DescriptorId,
            context,
            new CalculationInput(input));

        var result = _engine.Execute(request);

        return result.Output.Values;
    }

    internal static decimal ReadDecimal(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' was not found.");
        }

        return PrimitiveInput.ToDecimal(raw, key);
    }

    internal static bool ReadBoolean(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' was not found.");
        }

        return PrimitiveInput.ToBool(raw, key);
    }

    internal static string ReadString(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' was not found.");
        }

        var text = raw as string ?? raw.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' produced an empty value.");
        }

        return text.Trim();
    }

    internal static ImmutableArray<decimal> ReadDecimalArray(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' was not found.");
        }

        if (raw is IEnumerable<decimal> decimals)
        {
            return decimals.ToImmutableArray();
        }

        if (raw is IEnumerable<object?> objects)
        {
            return objects.Select((item, index) => PrimitiveInput.ToDecimal(item, $"{key}[{index}]"))
                .ToImmutableArray();
        }

        throw new CalculationOperationValidationException($"Expected output key '{key}' is not a decimal sequence.");
    }

    internal static ImmutableArray<int> ReadIntArray(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            throw new CalculationOperationValidationException($"Expected output key '{key}' was not found.");
        }

        if (raw is IEnumerable<int> ints)
        {
            return ints.ToImmutableArray();
        }

        if (raw is IEnumerable<object?> objects)
        {
            return objects.Select((item, index) => PrimitiveInput.ToInt(item, $"{key}[{index}]"))
                .ToImmutableArray();
        }

        throw new CalculationOperationValidationException($"Expected output key '{key}' is not an integer sequence.");
    }

    internal static ImmutableDictionary<string, object?> Input(params (string key, object? value)[] values)
    {
        return values.ToImmutableDictionary(item => item.key, item => item.value, StringComparer.OrdinalIgnoreCase);
    }

    internal static ImmutableArray<decimal> ToImmutableDecimalArray(IEnumerable<decimal> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values);

        var result = values.ToImmutableArray();
        if (result.Length == 0)
        {
            throw new CalculationOperationValidationException($"Input '{parameterName}' must contain at least one value.");
        }

        return result;
    }

    internal static decimal ToNonNegativeDecimal(decimal value, string parameterName)
    {
        if (value < 0m)
        {
            throw new CalculationOperationValidationException($"Input '{parameterName}' must be greater than or equal to zero.");
        }

        return value;
    }

    internal static int ToNonNegativeInt(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new CalculationOperationValidationException($"Input '{parameterName}' must be greater than or equal to zero.");
        }

        return value;
    }

    internal static decimal CeilingToDecimal(decimal value)
    {
        return decimal.Parse(Math.Ceiling(value).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
