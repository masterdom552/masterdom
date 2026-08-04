using System.Globalization;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Primitives;

internal static class PrimitiveCapabilityIds
{
    internal const string AggregationSum = "aggregation.sum";
    internal const string AggregationMean = "aggregation.mean";
    internal const string AggregationWeightedMean = "aggregation.weighted_mean";
    internal const string AggregationMin = "aggregation.min";
    internal const string AggregationMax = "aggregation.max";

    internal const string NormalizationClamp = "normalization.clamp";
    internal const string NormalizationRatio = "normalization.ratio";
    internal const string NormalizationBoundsGuard = "normalization.bounds_guard";

    internal const string StatisticsSpread = "statistics.spread";

    internal const string InterpolationWeightedBlend = "interpolation.weighted_blend";
    internal const string InterpolationReliabilityBlend = "interpolation.reliability_blend";

    internal const string ProjectionTrendFactor = "projection.trend_factor";
    internal const string ProjectionThresholdVariance = "projection.threshold_variance";

    internal const string ValidationThreshold = "validation.threshold";
    internal const string ValidationRange = "validation.range";

    internal const string RankingOrder = "ranking.order";
    internal const string RankingTopN = "ranking.top_n";
    internal const string RankingTieBreak = "ranking.tie_break";

    internal const string ScoringWeightedScore = "scoring.weighted_score";
    internal const string ScoringConfidence = "scoring.confidence";

    internal const string TransformationCanonicalDate = "transformation.canonical_date";
    internal const string TransformationCanonicalNumber = "transformation.canonical_number";
    internal const string TransformationCanonicalBoolean = "transformation.canonical_boolean";
}

internal abstract class CalculationPrimitiveOperationBase : ICalculationPrimitive
{
    private readonly CalculationOperationCapabilityCategory _capabilityCategory;
    private readonly CalculationOperationCompatibilityStatus _compatibilityStatus;

    protected CalculationPrimitiveOperationBase(
        string capabilityId,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationCompatibilityStatus compatibilityStatus = CalculationOperationCompatibilityStatus.Supported)
    {
        CapabilityId = capabilityId;
        _capabilityCategory = capabilityCategory;
        _compatibilityStatus = compatibilityStatus;
    }

    protected string CapabilityId { get; }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var outputValues = ExecuteCore(ParseInput(request));
        var output = new CalculationOutput(outputValues);

        var metadata = new CalculationExecutionMetadata(
            request.OperationId,
            CalculationOperationVersion.Create("1.0.0"),
            request.Context.EffectiveDateUtc,
            TimeSpan.Zero,
            CalculationOperationCapabilityId.Create(CapabilityId),
            _capabilityCategory,
            _compatibilityStatus);

        return new CalculationResult(output, metadata);
    }

    protected abstract object ParseInput(ICalculationRequest request);

    protected abstract IReadOnlyDictionary<string, object?> ExecuteCore(object input);
}

internal static class PrimitiveInput
{
    internal static decimal GetDecimal(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            throw new CalculationOperationValidationException($"Required input '{key}' was not provided.");
        }

        return ToDecimal(raw, key);
    }

    internal static decimal[] GetDecimalArray(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            throw new CalculationOperationValidationException($"Required input '{key}' was not provided.");
        }

        if (raw is null)
        {
            throw new CalculationOperationValidationException($"Input '{key}' cannot be null.");
        }

        if (raw is IEnumerable<decimal> decimalValues)
        {
            return decimalValues.ToArray();
        }

        if (raw is IEnumerable<object?> objectValues)
        {
            return objectValues.Select((value, index) => ToDecimal(value, $"{key}[{index}]"))
                .ToArray();
        }

        throw new CalculationOperationValidationException($"Input '{key}' must be a decimal sequence.");
    }

    internal static int GetInt(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            throw new CalculationOperationValidationException($"Required input '{key}' was not provided.");
        }

        return ToInt(raw, key);
    }

    internal static bool GetBool(IReadOnlyDictionary<string, object?> values, string key, bool defaultValue = false)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            return defaultValue;
        }

        if (raw is null)
        {
            return defaultValue;
        }

        return ToBool(raw, key);
    }

    internal static string GetString(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            throw new CalculationOperationValidationException($"Required input '{key}' was not provided.");
        }

        var text = raw as string ?? raw.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new CalculationOperationValidationException($"Input '{key}' cannot be empty.");
        }

        return text.Trim();
    }

    internal static object? GetAny(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            throw new CalculationOperationValidationException($"Required input '{key}' was not provided.");
        }

        return raw;
    }

    internal static void EnsureSameLength(decimal[] left, decimal[] right, string leftKey, string rightKey)
    {
        if (left.Length != right.Length)
        {
            throw new CalculationOperationValidationException(
                $"Input arrays '{leftKey}' and '{rightKey}' must have the same length.");
        }
    }

    internal static void EnsureNonEmpty(decimal[] values, string key)
    {
        if (values.Length == 0)
        {
            throw new CalculationOperationValidationException($"Input '{key}' must contain at least one value.");
        }
    }

    internal static decimal ToDecimal(object? raw, string key)
    {
        if (raw is null)
        {
            throw new CalculationOperationValidationException($"Input '{key}' cannot be null.");
        }

        try
        {
            return raw switch
            {
                decimal value => value,
                double value => Convert.ToDecimal(value, CultureInfo.InvariantCulture),
                float value => Convert.ToDecimal(value, CultureInfo.InvariantCulture),
                int value => value,
                long value => value,
                short value => value,
                byte value => value,
                uint value => value,
                ulong value => value,
                string text => decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture),
                _ => Convert.ToDecimal(raw, CultureInfo.InvariantCulture)
            };
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new CalculationOperationValidationException($"Input '{key}' is not a valid decimal value.");
        }
    }

    internal static int ToInt(object? raw, string key)
    {
        if (raw is null)
        {
            throw new CalculationOperationValidationException($"Input '{key}' cannot be null.");
        }

        try
        {
            return raw switch
            {
                int value => value,
                long value => checked((int)value),
                short value => value,
                byte value => value,
                string text => int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture),
                _ => Convert.ToInt32(raw, CultureInfo.InvariantCulture)
            };
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new CalculationOperationValidationException($"Input '{key}' is not a valid integer value.");
        }
    }

    internal static bool ToBool(object raw, string key)
    {
        if (raw is bool flag)
        {
            return flag;
        }

        if (raw is string text)
        {
            var trimmed = text.Trim();
            if (bool.TryParse(trimmed, out var parsed))
            {
                return parsed;
            }

            if (string.Equals(trimmed, "1", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(trimmed, "0", StringComparison.Ordinal))
            {
                return false;
            }

            throw new CalculationOperationValidationException($"Input '{key}' is not a valid boolean value.");
        }

        if (raw is int number)
        {
            return number switch
            {
                0 => false,
                1 => true,
                _ => throw new CalculationOperationValidationException($"Input '{key}' is not a valid boolean value.")
            };
        }

        throw new CalculationOperationValidationException($"Input '{key}' is not a valid boolean value.");
    }
}
