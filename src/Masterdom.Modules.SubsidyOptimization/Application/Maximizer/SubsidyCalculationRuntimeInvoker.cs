using Masterdom.Platform.CalculationEngine.Contracts;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public class SubsidyCalculationRuntimeInvoker
{
    private readonly ICalculationRuntime _runtime;

    public SubsidyCalculationRuntimeInvoker(ICalculationRuntime runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public virtual ICalculationResult Execute(
        string capabilityId,
        IReadOnlyDictionary<string, object?> input,
        DateTime effectiveDateUtc)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new ArgumentException("CapabilityId is required.", nameof(capabilityId));
        }

        ArgumentNullException.ThrowIfNull(input);

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("EffectiveDateUtc must be UTC.");
        }

        var request = new CalculationRuntimeRequest(
            CalculationCapabilityId.Create(capabilityId),
            new CalculationContext(effectiveDateUtc),
            new CalculationInput(input));

        return _runtime.Execute(request);
    }

    public static decimal ReadDecimal(ICalculationResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        return ToDecimal(ReadValue(result, key), key);
    }

    public static bool ReadBool(ICalculationResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        return ToBool(ReadValue(result, key), key);
    }

    public static string ReadString(ICalculationResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        var value = ReadValue(result, key);
        var text = value as string ?? value?.ToString();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException($"Calculation output '{key}' was empty.");
        }

        return text.Trim();
    }

    public static IReadOnlyList<int> ReadIntList(ICalculationResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        var value = ReadValue(result, key);

        if (value is IEnumerable<int> ints)
        {
            return ints.ToArray();
        }

        if (value is IEnumerable<object?> objects)
        {
            return objects.Select((item, index) => ToInt(item, $"{key}[{index}]")).ToArray();
        }

        throw new InvalidOperationException($"Calculation output '{key}' was not an integer sequence.");
    }

    private static object? ReadValue(ICalculationResult result, string key)
    {
        if (!result.Output.Values.TryGetValue(key, out var value))
        {
            throw new InvalidOperationException($"Calculation output '{key}' was not found.");
        }

        return value;
    }

    private static decimal ToDecimal(object? raw, string key)
    {
        if (raw is null)
        {
            throw new InvalidOperationException($"Calculation output '{key}' was null.");
        }

        try
        {
            return raw switch
            {
                decimal value => value,
                double value => Convert.ToDecimal(value),
                float value => Convert.ToDecimal(value),
                int value => value,
                long value => value,
                string text => decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
                _ => Convert.ToDecimal(raw)
            };
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new InvalidOperationException($"Calculation output '{key}' was not a valid decimal value.");
        }
    }

    private static int ToInt(object? raw, string key)
    {
        if (raw is null)
        {
            throw new InvalidOperationException($"Calculation output '{key}' was null.");
        }

        try
        {
            return raw switch
            {
                int value => value,
                long value => checked((int)value),
                short value => value,
                byte value => value,
                string text => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture),
                _ => Convert.ToInt32(raw)
            };
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new InvalidOperationException($"Calculation output '{key}' was not a valid integer value.");
        }
    }

    private static bool ToBool(object? raw, string key)
    {
        if (raw is null)
        {
            throw new InvalidOperationException($"Calculation output '{key}' was null.");
        }

        if (raw is bool value)
        {
            return value;
        }

        if (raw is string text)
        {
            if (bool.TryParse(text, out var parsed))
            {
                return parsed;
            }
        }

        throw new InvalidOperationException($"Calculation output '{key}' was not a valid boolean value.");
    }
}
