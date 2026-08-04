using System.Collections.Generic;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Manages the runtime configuration record lifecycle.
/// </summary>
public interface IConfigurationRegistry
{
    /// <summary>
    /// Replaces all configuration records atomically after validation.
    /// </summary>
    void ReplaceAll(IReadOnlyList<ConfigurationRecord> records);

    /// <summary>
    /// Registers additional records after validation.
    /// </summary>
    void RegisterRange(IReadOnlyList<ConfigurationRecord> records);

    /// <summary>
    /// Gets the current immutable snapshot of records.
    /// </summary>
    IReadOnlyList<ConfigurationRecord> GetAll();
}
