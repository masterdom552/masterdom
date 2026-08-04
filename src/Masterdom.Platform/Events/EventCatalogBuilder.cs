using System;
using System.Collections.Generic;
using Masterdom.Platform.Modules;

namespace Masterdom.Platform.Events;

/// <summary>
/// Builds initial platform event descriptors from module catalog entries.
/// </summary>
public static class EventCatalogBuilder
{
    public static IReadOnlyList<EventDescriptor> BuildFromCatalog(
        IEnumerable<ModuleCatalogEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var descriptors = new List<EventDescriptor>();

        foreach (var entry in entries)
        {
            descriptors.Add(new EventDescriptor
            {
                EventType = new EventType($"platform.module.{entry.ModuleId}.loaded"),
                Category = EventCategory.Lifecycle,
                Version = new EventVersion(1),
                RequiresHandler = false
            });

            descriptors.Add(new EventDescriptor
            {
                EventType = new EventType($"platform.module.{entry.ModuleId}.initialized"),
                Category = EventCategory.Lifecycle,
                Version = new EventVersion(1),
                RequiresHandler = false
            });

            descriptors.Add(new EventDescriptor
            {
                EventType = new EventType($"platform.module.{entry.ModuleId}.shutdown"),
                Category = EventCategory.Lifecycle,
                Version = new EventVersion(1),
                RequiresHandler = false
            });
        }

        return descriptors;
    }
}
