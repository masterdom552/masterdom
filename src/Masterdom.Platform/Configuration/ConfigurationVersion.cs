namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents a configuration version number.
/// </summary>
public readonly struct ConfigurationVersion
{
    public ConfigurationVersion(int value)
    {
        if (value <= 0)
        {
            throw new PlatformConfigurationValidationException(
                "Configuration version must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
