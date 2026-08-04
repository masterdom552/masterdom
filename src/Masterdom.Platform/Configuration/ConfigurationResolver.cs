using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Resolves effective configuration values by scope precedence and effective dating.
/// </summary>
public sealed class ConfigurationResolver : IConfigurationResolver
{
    private readonly IConfigurationRepository _repository;
    private readonly IConfigurationDefaults _defaults;

    public ConfigurationResolver(
        IConfigurationRepository repository,
        IConfigurationDefaults? defaults = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _defaults = defaults ?? new DefaultConfigurationDefaults();
    }

    public ConfigurationResolutionResult Resolve(
        ConfigurationKey key,
        ConfigurationResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ModuleId))
        {
            throw new PlatformConfigurationValidationException(
                "ModuleId is required for configuration resolution.");
        }

        if (request.AsOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new PlatformConfigurationValidationException(
                "AsOfUtc must be UTC.");
        }

        var records = _repository.GetAll();
        var effectiveRecords = records
            .Where(record => record.Key.Equals(key))
            .Where(record => record.Period.IsEffectiveAt(request.AsOfUtc))
            .ToList();

        ConfigurationValidation.ValidateNoActiveOverlaps(
            effectiveRecords,
            request.AsOfUtc);

        var scopes = BuildScopeChain(request);

        foreach (var scope in scopes)
        {
            var selected = SelectLatest(effectiveRecords, scope);
            if (selected is not null)
            {
                return new ConfigurationResolutionResult
                {
                    Record = selected,
                    IsDefault = false
                };
            }
        }

        var defaults = _defaults.GetDefaults();
        if (!defaults.TryGetValue(key, out var defaultValue))
        {
            throw new PlatformConfigurationValidationException(
                $"Configuration value not found for key '{key.Value}'.");
        }

        var defaultRecord = new ConfigurationRecord(
            new ConfigurationId(Guid.NewGuid()),
            key,
            ConfigurationScope.Global(),
            new ConfigurationVersion(1),
            defaultValue,
            new EffectivePeriod(DateTime.UnixEpoch, null),
            "system-default",
            "Default fallback",
            DateTime.UnixEpoch);

        return new ConfigurationResolutionResult
        {
            Record = defaultRecord,
            IsDefault = true
        };
    }

    private static ConfigurationRecord? SelectLatest(
        IEnumerable<ConfigurationRecord> records,
        ConfigurationScope scope)
    {
        return records
            .Where(record => record.Scope.Equals(scope))
            .OrderByDescending(record => record.Period.EffectiveFromUtc)
            .ThenByDescending(record => record.Version.Value)
            .FirstOrDefault();
    }

    private static IReadOnlyList<ConfigurationScope> BuildScopeChain(
        ConfigurationResolutionRequest request)
    {
        var scopes = new List<ConfigurationScope>();

        if (!string.IsNullOrWhiteSpace(request.PropertyId))
        {
            scopes.Add(ConfigurationScope.Property(request.PropertyId));
        }

        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            scopes.Add(ConfigurationScope.Tenant(request.TenantId));
        }

        scopes.Add(ConfigurationScope.Module(request.ModuleId));
        scopes.Add(ConfigurationScope.Global());

        return scopes;
    }

}
