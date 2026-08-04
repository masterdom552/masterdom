using System;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.Tests.Configuration;

public sealed class ConfigurationRegistryTests
{
    [Fact]
    public void ReplaceAll_WhenOverlappingPeriodsExist_ShouldThrow()
    {
        var registry = new ConfigurationRegistry();
        var key = new ConfigurationKey("billing.penalty-rate");
        var fromUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        Assert.Throws<PlatformConfigurationValidationException>(() =>
            registry.ReplaceAll(new[]
            {
                CreateRecord(
                    key,
                    ConfigurationScope.Module("billing"),
                    "0.01",
                    fromUtc,
                    version: 1,
                    toUtc: fromUtc.AddDays(10)),
                CreateRecord(
                    key,
                    ConfigurationScope.Module("billing"),
                    "0.02",
                    fromUtc.AddDays(5),
                    version: 2)
            }));
    }

    [Fact]
    public void RegisterRange_WhenRecordsAreValid_ShouldAppendToRepository()
    {
        var registry = new ConfigurationRegistry();
        var key = new ConfigurationKey("billing.penalty-rate");
        var fromUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);

        registry.ReplaceAll(new[]
        {
            CreateRecord(
                key,
                ConfigurationScope.Module("billing"),
                "0.01",
                fromUtc,
                version: 1,
                toUtc: fromUtc.AddDays(10))
        });

        registry.RegisterRange(new[]
        {
            CreateRecord(
                key,
                ConfigurationScope.Module("billing"),
                "0.02",
                fromUtc.AddDays(10),
                version: 2)
        });

        var records = registry.GetAll();

        Assert.Equal(2, records.Count);
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
}
