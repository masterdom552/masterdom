using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Builds startup graphs from module catalogs.
/// </summary>
public static class ModuleStartupGraphBuilder
{
    /// <summary>
    /// Builds a startup graph from the specified catalog.
    /// </summary>
    public static ModuleStartupGraph Build(IPlatformModuleCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        ModuleCatalogValidator.Validate(catalog);

        var entries = catalog.Entries;
        var byId = entries.ToDictionary(
            e => e.ModuleId,
            StringComparer.OrdinalIgnoreCase);

        var dependents = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            dependents[entry.ModuleId] = new List<string>();
            inDegree[entry.ModuleId] = 0;
        }

        foreach (var entry in entries)
        {
            foreach (var dependency in entry.Dependencies)
            {
                dependents[dependency.ModuleId].Add(entry.ModuleId);
                inDegree[entry.ModuleId]++;
            }
        }

        var ready = new SortedSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (inDegree[entry.ModuleId] == 0)
            {
                ready.Add(entry.ModuleId);
            }
        }

        var ordered = new List<ModuleCatalogEntry>(entries.Count);

        while (ready.Count > 0)
        {
            var nextId = ready
                .Select(id => byId[id])
                .OrderBy(entry => entry.StartupOrder)
                .ThenBy(entry => entry.ModuleId, StringComparer.OrdinalIgnoreCase)
                .First()
                .ModuleId;

            ready.Remove(nextId);

            ordered.Add(byId[nextId]);

            foreach (var dependentId in dependents[nextId])
            {
                inDegree[dependentId]--;

                if (inDegree[dependentId] == 0)
                {
                    ready.Add(dependentId);
                }
            }
        }

        if (ordered.Count != entries.Count)
        {
            var remaining = inDegree
                .Where(pair => pair.Value > 0)
                .Select(pair => pair.Key)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            throw new ModuleCatalogValidationException(
                $"Circular dependencies detected in module catalog: {string.Join(", ", remaining)}.");
        }

        var dependenciesByModule = entries.ToDictionary(
            e => e.ModuleId,
            e => (IReadOnlyList<string>)e.Dependencies
                .Select(dependency => dependency.ModuleId)
                .ToList(),
            StringComparer.OrdinalIgnoreCase);

        return new ModuleStartupGraph
        {
            OrderedModules = ordered,
            DependenciesByModule = dependenciesByModule
        };
    }
}
