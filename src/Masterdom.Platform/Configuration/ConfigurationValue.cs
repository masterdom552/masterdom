using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents a validated configuration value.
/// </summary>
public sealed class ConfigurationValue
{
    public ConfigurationValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new PlatformConfigurationValidationException(
                "Configuration value is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}
