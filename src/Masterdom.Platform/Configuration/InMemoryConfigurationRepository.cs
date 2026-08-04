using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Represents an in-memory configuration repository.
/// </summary>
public sealed class InMemoryConfigurationRepository : IConfigurationRepository
{
    private List<ConfigurationRecord> _records;

    public InMemoryConfigurationRepository(IReadOnlyList<ConfigurationRecord>? records = null)
    {
        _records = records?.ToList() ?? new List<ConfigurationRecord>();
    }

    public IReadOnlyList<ConfigurationRecord> GetAll()
    {
        return _records;
    }

    public void ReplaceAll(IReadOnlyList<ConfigurationRecord> records)
    {
        _records = records?.ToList() ?? throw new ArgumentNullException(nameof(records));
    }
}
