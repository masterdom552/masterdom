using System.Collections.Generic;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Provides read access to versioned configuration records.
/// </summary>
public interface IConfigurationRepository
{
    IReadOnlyList<ConfigurationRecord> GetAll();
}
