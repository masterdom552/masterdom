using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Modules;

/// <summary>
/// In-memory implementation of <see cref="IPlatformModuleCatalog"/>.
/// </summary>
public sealed class PlatformModuleCatalog : IPlatformModuleCatalog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformModuleCatalog"/> class.
    /// </summary>
    public PlatformModuleCatalog(IEnumerable<ModuleCatalogEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = entries.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ModuleCatalogEntry> Entries { get; }
}
