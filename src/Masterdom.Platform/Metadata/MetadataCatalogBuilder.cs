using System;
using System.Collections.Generic;
using Masterdom.Platform.Modules;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Converts module catalog entries into metadata definitions.
/// </summary>
public static class MetadataCatalogBuilder
{
    public static IReadOnlyList<MetadataDefinition> BuildFromCatalog(
        IEnumerable<ModuleCatalogEntry> entries,
        DateTime? effectiveFromUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var list = new List<MetadataDefinition>();
        var fromUtc = effectiveFromUtc ?? DateTime.UnixEpoch;

        foreach (var entry in entries)
        {
            list.Add(new MetadataDefinition(
                new MetadataId(Guid.NewGuid()),
                new MetadataKey($"module.{entry.ModuleId}"),
                MetadataCategory.Module,
                MetadataScope.Module(entry.ModuleId),
                new MetadataVersion(1),
                new MetadataEffectivePeriod(fromUtc, null),
                entry.Module.Metadata.DisplayName,
                entry.Module.Metadata.Description,
                null,
                false,
                null,
                $"CatalogVersion:{entry.Version}",
                "catalog",
                fromUtc));
        }

        return list;
    }
}
