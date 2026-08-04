using System;
using System.Collections.Generic;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.Tests.Configuration;

public sealed class ConfigurationResolverTests
{
    [Fact]
    public void ConfigurationVersion_WhenValueIsNotPositive_ShouldThrow()
    {
        Assert.Throws<PlatformConfigurationValidationException>(() =>
            new ConfigurationVersion(0));
    }

    [Fact]
    public void Resolve_ShouldPreferPropertyThenTenantThenModuleThenGlobal()
    {
        var key = new ConfigurationKey("billing.penalty-rate");
        var now = DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc);

        var repository = new InMemoryConfigurationRepository(new List<ConfigurationRecord>
        {
            CreateRecord(key, ConfigurationScope.Global(), "0.01", now.AddDays(-30), 1),
            CreateRecord(key, ConfigurationScope.Module("billing"), "0.02", now.AddDays(-20), 1),
            CreateRecord(key, ConfigurationScope.Tenant("tenant-a"), "0.03", now.AddDays(-10), 1),
            CreateRecord(key, ConfigurationScope.Property("property-a"), "0.05", now.AddDays(-5), 1)
        });

        var resolver = new ConfigurationResolver(repository);

        var result = resolver.Resolve(
            key,
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                TenantId = "tenant-a",
                PropertyId = "property-a",
                AsOfUtc = now
            });

        Assert.Equal("0.05", result.Record.Value.Value);
        Assert.False(result.IsDefault);
    }

    [Fact]
    public void Resolve_ShouldSelectLatestVersionByEffectiveDateWithinSameScope()
    {
        var key = new ConfigurationKey("billing.billing-day");
        var now = DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc);

        var repository = new InMemoryConfigurationRepository(new List<ConfigurationRecord>
        {
            CreateRecord(key, ConfigurationScope.Module("billing"), "5", now.AddDays(-60), 1, now.AddDays(-10)),
            CreateRecord(key, ConfigurationScope.Module("billing"), "10", now.AddDays(-10), 2)
        });

        var resolver = new ConfigurationResolver(repository);

        var result = resolver.Resolve(
            key,
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = now
            });

        Assert.Equal("10", result.Record.Value.Value);
        Assert.Equal(2, result.Record.Version.Value);
    }

    [Fact]
    public void Resolve_WhenMissingInRepository_ShouldUseDefaults()
    {
        var key = new ConfigurationKey("billing.currency");
        var defaults = new DictionaryConfigurationDefaults(new Dictionary<ConfigurationKey, ConfigurationValue>
        {
            [key] = new ConfigurationValue("USD")
        });

        var resolver = new ConfigurationResolver(
            new InMemoryConfigurationRepository(),
            defaults);

        var result = resolver.Resolve(
            key,
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc)
            });

        Assert.Equal("USD", result.Record.Value.Value);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public void Resolve_WhenOverlappingActiveVersionsExist_ShouldThrow()
    {
        var key = new ConfigurationKey("billing.grace-days");
        var now = DateTime.SpecifyKind(new DateTime(2026, 3, 1), DateTimeKind.Utc);

        var repository = new InMemoryConfigurationRepository(new List<ConfigurationRecord>
        {
            CreateRecord(key, ConfigurationScope.Module("billing"), "7", now.AddDays(-20), 1),
            CreateRecord(key, ConfigurationScope.Module("billing"), "9", now.AddDays(-10), 2)
        });

        var resolver = new ConfigurationResolver(repository);

        Assert.Throws<PlatformConfigurationValidationException>(() =>
            resolver.Resolve(
                key,
                new ConfigurationResolutionRequest
                {
                    ModuleId = "billing",
                    AsOfUtc = now
                }));
    }

    [Fact]
    public void Resolve_WhenDuplicateActiveRecordsExistAtSameScope_ShouldThrow()
    {
        var key = new ConfigurationKey("billing.duplicate-key");
        var now = DateTime.SpecifyKind(new DateTime(2026, 3, 15), DateTimeKind.Utc);

        var repository = new InMemoryConfigurationRepository(new List<ConfigurationRecord>
        {
            CreateRecord(key, ConfigurationScope.Module("billing"), "a", now.AddDays(-5), 1),
            CreateRecord(key, ConfigurationScope.Module("billing"), "b", now.AddDays(-4), 1)
        });

        var resolver = new ConfigurationResolver(repository);

        Assert.Throws<PlatformConfigurationValidationException>(() =>
            resolver.Resolve(
                key,
                new ConfigurationResolutionRequest
                {
                    ModuleId = "billing",
                    AsOfUtc = now
                }));
    }

    private static ConfigurationRecord CreateRecord(
        ConfigurationKey key,
        ConfigurationScope scope,
        string value,
        DateTime fromUtc,
        int version,
        DateTime? toUtc = null)
    {
        return new ConfigurationRecord(
            new ConfigurationId(Guid.NewGuid()),
            key,
            scope,
            new ConfigurationVersion(version),
            new ConfigurationValue(value),
            new EffectivePeriod(fromUtc, toUtc),
            "tester",
            "test setup",
            fromUtc);
    }

    private sealed class DictionaryConfigurationDefaults : IConfigurationDefaults
    {
        private readonly IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> _values;

        public DictionaryConfigurationDefaults(IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> values)
        {
            _values = values;
        }

        public IReadOnlyDictionary<ConfigurationKey, ConfigurationValue> GetDefaults()
        {
            return _values;
        }
    }
}
