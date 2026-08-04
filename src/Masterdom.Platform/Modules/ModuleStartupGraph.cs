using System.Collections.Generic;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the deterministic startup graph generated from a module catalog.
/// </summary>
public sealed class ModuleStartupGraph
{
    /// <summary>
    /// Gets modules in startup order.
    /// </summary>
    public required IReadOnlyList<ModuleCatalogEntry> OrderedModules { get; init; }

    /// <summary>
    /// Gets module dependency edges keyed by module id.
    /// </summary>
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> DependenciesByModule { get; init; }
}
