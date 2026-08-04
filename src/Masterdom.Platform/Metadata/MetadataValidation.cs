using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Metadata;

/// <summary>
/// Performs metadata framework validation rules.
/// </summary>
public static class MetadataValidation
{
    public static void ValidateAll(IReadOnlyList<MetadataDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        ValidateDuplicateIdentifiers(definitions);
        ValidateDuplicateKeys(definitions);
        ValidateInvalidScopes(definitions);
        ValidateMissingParents(definitions);
        ValidateCircularReferences(definitions);
        ValidateInvalidInheritance(definitions);
    }

    private static void ValidateDuplicateIdentifiers(IReadOnlyList<MetadataDefinition> definitions)
    {
        var duplicates = definitions
            .GroupBy(definition => definition.Id.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new MetadataValidationException(
                $"Duplicate metadata identifiers were found: {string.Join(", ", duplicates)}.");
        }
    }

    private static void ValidateDuplicateKeys(IReadOnlyList<MetadataDefinition> definitions)
    {
        var duplicates = definitions
            .GroupBy(
                definition => new
                {
                    Key = definition.Key.Value.ToUpperInvariant(),
                    Scope = definition.Scope.ToString().ToUpperInvariant(),
                    Version = definition.Version.Value,
                    EffectiveFrom = definition.Period.EffectiveFromUtc
                })
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new MetadataValidationException(
                "Duplicate metadata keys were found for the same scope, version, and effective date.");
        }
    }

    private static void ValidateInvalidScopes(IReadOnlyList<MetadataDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            if (IsScopeAllowed(definition.Category, definition.Scope.Kind))
            {
                continue;
            }

            throw new MetadataValidationException(
                $"Invalid metadata scope '{definition.Scope.Kind}' for category '{definition.Category}' and key '{definition.Key.Value}'.");
        }
    }

    private static void ValidateMissingParents(IReadOnlyList<MetadataDefinition> definitions)
    {
        var ids = definitions
            .Select(definition => definition.Id.Value)
            .ToHashSet();

        foreach (var definition in definitions)
        {
            if (!definition.ParentId.HasValue)
            {
                continue;
            }

            if (ids.Contains(definition.ParentId.Value.Value))
            {
                continue;
            }

            throw new MetadataValidationException(
                $"Metadata parent '{definition.ParentId.Value.Value}' was not found for '{definition.Key.Value}'.");
        }
    }

    private static void ValidateCircularReferences(IReadOnlyList<MetadataDefinition> definitions)
    {
        var byId = definitions.ToDictionary(definition => definition.Id.Value);

        foreach (var definition in definitions)
        {
            var visited = new HashSet<Guid>();
            var cursor = definition;

            while (cursor.ParentId.HasValue)
            {
                if (!visited.Add(cursor.Id.Value))
                {
                    throw new MetadataValidationException(
                        $"Circular metadata inheritance detected at '{cursor.Id.Value}'.");
                }

                if (!byId.TryGetValue(cursor.ParentId.Value.Value, out var parent))
                {
                    break;
                }

                cursor = parent;
            }
        }
    }

    private static void ValidateInvalidInheritance(IReadOnlyList<MetadataDefinition> definitions)
    {
        var byId = definitions.ToDictionary(definition => definition.Id.Value);

        foreach (var definition in definitions)
        {
            if (!definition.ParentId.HasValue)
            {
                continue;
            }

            var parent = byId[definition.ParentId.Value.Value];

            if (IsInheritanceAllowed(parent.Category, definition.Category))
            {
                continue;
            }

            throw new MetadataValidationException(
                $"Invalid metadata inheritance: '{definition.Category}' cannot inherit from '{parent.Category}'.");
        }
    }

    private static bool IsScopeAllowed(MetadataCategory category, MetadataScopeKind scopeKind)
    {
        return category switch
        {
            MetadataCategory.Module => scopeKind is MetadataScopeKind.Global or MetadataScopeKind.Module,
            MetadataCategory.Aggregate => scopeKind is MetadataScopeKind.Aggregate or MetadataScopeKind.Module,
            MetadataCategory.Entity => scopeKind is MetadataScopeKind.Entity or MetadataScopeKind.Aggregate,
            MetadataCategory.Property => scopeKind is MetadataScopeKind.Property or MetadataScopeKind.Entity,
            MetadataCategory.Field => scopeKind is MetadataScopeKind.Field or MetadataScopeKind.Property,
            MetadataCategory.Enumeration => scopeKind is MetadataScopeKind.Enumeration or MetadataScopeKind.Field,
            MetadataCategory.Validation => scopeKind is MetadataScopeKind.Global or MetadataScopeKind.Module or MetadataScopeKind.Aggregate or MetadataScopeKind.Entity or MetadataScopeKind.Property or MetadataScopeKind.Field,
            MetadataCategory.Ui => scopeKind is MetadataScopeKind.Global or MetadataScopeKind.Module or MetadataScopeKind.Entity or MetadataScopeKind.Property or MetadataScopeKind.Field,
            MetadataCategory.Reporting => scopeKind is MetadataScopeKind.Global or MetadataScopeKind.Module or MetadataScopeKind.Aggregate or MetadataScopeKind.Entity,
            MetadataCategory.Search => scopeKind is MetadataScopeKind.Global or MetadataScopeKind.Module or MetadataScopeKind.Aggregate or MetadataScopeKind.Entity or MetadataScopeKind.Property,
            _ => false
        };
    }

    private static bool IsInheritanceAllowed(MetadataCategory parent, MetadataCategory child)
    {
        if (parent == child)
        {
            return true;
        }

        return parent switch
        {
            MetadataCategory.Module => child is MetadataCategory.Aggregate or MetadataCategory.Entity or MetadataCategory.Property or MetadataCategory.Field or MetadataCategory.Enumeration or MetadataCategory.Validation or MetadataCategory.Ui or MetadataCategory.Reporting or MetadataCategory.Search,
            MetadataCategory.Aggregate => child is MetadataCategory.Entity or MetadataCategory.Property or MetadataCategory.Field or MetadataCategory.Validation or MetadataCategory.Reporting or MetadataCategory.Search,
            MetadataCategory.Entity => child is MetadataCategory.Property or MetadataCategory.Field or MetadataCategory.Validation or MetadataCategory.Ui or MetadataCategory.Reporting or MetadataCategory.Search,
            MetadataCategory.Property => child is MetadataCategory.Field or MetadataCategory.Enumeration or MetadataCategory.Validation or MetadataCategory.Ui or MetadataCategory.Search,
            MetadataCategory.Field => child is MetadataCategory.Enumeration or MetadataCategory.Validation or MetadataCategory.Ui,
            MetadataCategory.Enumeration => child is MetadataCategory.Validation or MetadataCategory.Ui,
            MetadataCategory.Validation => child is MetadataCategory.Ui,
            MetadataCategory.Ui => false,
            MetadataCategory.Reporting => false,
            MetadataCategory.Search => false,
            _ => false
        };
    }
}
