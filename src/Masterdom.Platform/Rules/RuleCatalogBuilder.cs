using System;
using System.Collections.Generic;
using Masterdom.Platform.Modules;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Converts module catalog entries into initial rule-set definitions.
/// </summary>
public static class RuleCatalogBuilder
{
    public static RuleCatalogSeed BuildFromCatalog(
        IEnumerable<ModuleCatalogEntry> entries,
        DateTime? effectiveFromUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var ruleSets = new List<RuleSetDefinition>();
        var rules = new List<RuleDefinition>();
        var fromUtc = effectiveFromUtc ?? DateTime.UnixEpoch;

        foreach (var entry in entries)
        {
            ruleSets.Add(new RuleSetDefinition(
                new RuleSetId(Guid.NewGuid()),
                new RuleSetKey($"rules.{entry.ModuleId}.default"),
                $"{entry.Module.Metadata.DisplayName} Default Rules",
                "Catalog-seeded default rule set.",
                RuleCategory.Custom,
                RuleScope.Create(RuleScopeKind.Module, entry.ModuleId),
                new RuleVersion(1),
                new RuleEffectivePeriod(fromUtc, null),
                false,
                null,
                $"CatalogVersion:{entry.Version}",
                "catalog",
                fromUtc));
        }

        return new RuleCatalogSeed
        {
            RuleSets = ruleSets,
            Rules = rules
        };
    }
}
