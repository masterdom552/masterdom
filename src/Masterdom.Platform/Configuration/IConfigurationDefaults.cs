using System.Collections.Generic;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Provides global fallback defaults for configuration keys.
/// </summary>
public interface IConfigurationDefaults
{
    IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> GetDefaults();
}
