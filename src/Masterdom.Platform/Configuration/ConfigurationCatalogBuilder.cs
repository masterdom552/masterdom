using System;
using System.Collections.Generic;
using Masterdom.Platform.Modules;

namespace Masterdom.Platform.Configuration;

/// <summary>
/// Converts module-catalog key-value pairs into versioned global configuration records.
/// </summary>
public static class ConfigurationCatalogBuilder
{
    public static IReadOnlyList<ConfigurationRecord> BuildFromCatalog(
        IEnumerable<ModuleCatalogEntry> entries,
        DateTime? effectiveFromUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var list = new List<ConfigurationRecord>();
        var fromUtc = effectiveFromUtc ?? DateTime.UnixEpoch;

        foreach (var entry in entries)
        {
            foreach (var pair in entry.Configuration)
            {
                var key = new ConfigurationKey($"{entry.ModuleId}.{pair.Key}");
                var value = new ConfigurationValue(pair.Value);

                list.Add(new ConfigurationRecord(
                    new ConfigurationId(Guid.NewGuid()),
                    key,
                    ConfigurationScope.Module(entry.ModuleId),
                    new ConfigurationVersion(1),
                    value,
                    new EffectivePeriod(fromUtc, null),
                    "catalog",
                    "Seeded from module catalog",
                    fromUtc));
            }
        }

        return list;
    }
}
