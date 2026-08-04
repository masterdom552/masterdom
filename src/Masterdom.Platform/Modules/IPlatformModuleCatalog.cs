using System.Collections.Generic;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the authoritative module catalog used by the platform runtime.
/// </summary>
public interface IPlatformModuleCatalog
{
    /// <summary>
    /// Gets all catalog entries.
    /// </summary>
    IReadOnlyList<ModuleCatalogEntry> Entries { get; }
}
