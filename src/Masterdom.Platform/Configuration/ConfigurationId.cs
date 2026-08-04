using System;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents the unique identity of a configuration record.
/// </summary>
public readonly struct ConfigurationId
{
    public ConfigurationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new PlatformConfigurationValidationException(
                "ConfigurationId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
