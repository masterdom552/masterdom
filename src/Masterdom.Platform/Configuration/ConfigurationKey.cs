using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents a normalized configuration key.
/// </summary>
public sealed class ConfigurationKey : IEquatable<ConfigurationKey>
{
    public ConfigurationKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new PlatformConfigurationValidationException(
                "Configuration key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(ConfigurationKey? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConfigurationKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Value;
    }
}
