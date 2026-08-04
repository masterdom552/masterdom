using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Default configuration registry for runtime configuration lifecycle.
/// </summary>
public sealed class ConfigurationRegistry : IConfigurationRegistry
{
    private readonly InMemoryConfigurationRepository _repository;

    public ConfigurationRegistry(IConfigurationRepository? repository = null)
    {
        _repository = repository as InMemoryConfigurationRepository
            ?? new InMemoryConfigurationRepository(repository?.GetAll());
    }

    public void ReplaceAll(IReadOnlyList<ConfigurationRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        ConfigurationValidation.ValidateForStorage(records);

        _repository.ReplaceAll(records);
    }

    public void RegisterRange(IReadOnlyList<ConfigurationRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var merged = _repository.GetAll()
            .Concat(records)
            .ToList();

        ConfigurationValidation.ValidateForStorage(merged);

        _repository.ReplaceAll(merged);
    }

    public IReadOnlyList<ConfigurationRecord> GetAll()
    {
        return _repository.GetAll();
    }
}
