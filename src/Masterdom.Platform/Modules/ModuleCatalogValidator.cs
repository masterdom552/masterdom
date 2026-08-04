using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Validates module catalog constraints before startup graph generation.
/// </summary>
public static class ModuleCatalogValidator
{
    /// <summary>
    /// Validates the specified catalog.
    /// </summary>
    public static void Validate(IPlatformModuleCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var entries = catalog.Entries;

        if (entries.Count == 0)
        {
            throw new ModuleCatalogValidationException(
                "Module catalog must contain at least one entry.");
        }

        ValidateDuplicateModuleInstances(entries);
        ValidateDuplicateIdentifiers(entries);
        ValidateModuleIdentity(entries);
        ValidateDependencies(entries);
        ValidateStartupOrder(entries);
    }

    private static void ValidateDuplicateModuleInstances(
        IReadOnlyList<ModuleCatalogEntry> entries)
    {
        var seen = new HashSet<IModule>(ReferenceEqualityComparer.Instance);

        foreach (var entry in entries)
        {
            if (!seen.Add(entry.Module))
            {
                throw new ModuleCatalogValidationException(
                    $"Duplicate module instance detected for module id '{entry.ModuleId}'.");
            }
        }
    }

    private static void ValidateDuplicateIdentifiers(
        IReadOnlyList<ModuleCatalogEntry> entries)
    {
        var byModuleId = entries
            .GroupBy(e => e.ModuleId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (byModuleId.Count > 0)
        {
            throw new ModuleCatalogValidationException(
                $"Duplicate module identifiers found: {string.Join(", ", byModuleId)}.");
        }
    }

    private static void ValidateModuleIdentity(
        IReadOnlyList<ModuleCatalogEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Module is null)
            {
                throw new ModuleCatalogValidationException(
                    $"Module instance is required for catalog entry '{entry.ModuleId}'.");
            }

            if (string.IsNullOrWhiteSpace(entry.ModuleId))
            {
                throw new ModuleCatalogValidationException(
                    "ModuleId is required for every catalog entry.");
            }

            if (string.IsNullOrWhiteSpace(entry.Version))
            {
                throw new ModuleCatalogValidationException(
                    $"Version is required for module '{entry.ModuleId}'.");
            }

            if (!string.Equals(
                    entry.ModuleId,
                    entry.Module.Metadata.Id,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ModuleCatalogValidationException(
                    $"Catalog id '{entry.ModuleId}' does not match module metadata id '{entry.Module.Metadata.Id}'.");
            }

            if (!string.Equals(
                    entry.Version,
                    entry.Module.Metadata.Version,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ModuleCatalogValidationException(
                    $"Catalog version '{entry.Version}' does not match module metadata version '{entry.Module.Metadata.Version}' for module '{entry.ModuleId}'.");
            }
        }
    }

    private static void ValidateDependencies(
        IReadOnlyList<ModuleCatalogEntry> entries)
    {
        var byId = entries.ToDictionary(
            e => e.ModuleId,
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            foreach (var dependency in entry.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency.ModuleId))
                {
                    throw new ModuleCatalogValidationException(
                        $"Module '{entry.ModuleId}' contains a dependency with an empty ModuleId.");
                }

                if (!byId.TryGetValue(dependency.ModuleId, out var target))
                {
                    throw new ModuleCatalogValidationException(
                        $"Module '{entry.ModuleId}' depends on missing module '{dependency.ModuleId}'.");
                }

                if (string.IsNullOrWhiteSpace(dependency.RequiredVersion))
                {
                    continue;
                }

                if (!string.Equals(
                        dependency.RequiredVersion,
                        target.Version,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ModuleCatalogValidationException(
                        $"Module '{entry.ModuleId}' requires '{dependency.ModuleId}' version '{dependency.RequiredVersion}', but catalog has version '{target.Version}'.");
                }
            }
        }
    }

    private static void ValidateStartupOrder(IReadOnlyList<ModuleCatalogEntry> entries)
    {
        var byId = entries.ToDictionary(
            e => e.ModuleId,
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            foreach (var dependency in entry.Dependencies)
            {
                var dependencyEntry = byId[dependency.ModuleId];

                if (entry.StartupOrder < dependencyEntry.StartupOrder)
                {
                    throw new ModuleCatalogValidationException(
                        $"Startup order conflict: module '{entry.ModuleId}' (order {entry.StartupOrder}) depends on '{dependencyEntry.ModuleId}' (order {dependencyEntry.StartupOrder}).");
                }
            }
        }
    }
}
