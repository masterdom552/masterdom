using System.Collections.Generic;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents an empty default source.
/// </summary>
public sealed class DefaultConfigurationDefaults : IConfigurationDefaults
{
    private static readonly IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> Defaults =
        new Dictionary<ConfigurationKey, ConfigurationValue>();

    public IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> GetDefaults()
    {
        return Defaults;
    }
}
