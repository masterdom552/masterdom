using System.Collections.Generic;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents a read-only catalog of platform modules.
/// </summary>
public interface IModuleCatalog : IEnumerable<IModule>
{
    /// <summary>
    /// Gets the number of registered modules.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Determines whether a module exists.
    /// </summary>
    bool Contains(string id);

    /// <summary>
    /// Gets a module by identifier.
    /// </summary>
    IModule Get(string id);

    /// <summary>
    /// Attempts to retrieve a module.
    /// </summary>
    bool TryGet(string id, out IModule? module);
}