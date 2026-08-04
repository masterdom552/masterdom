using System;
using System.Collections;
using System.Collections.Generic;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Stores platform modules.
/// </summary>
public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules =
        new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public int Count => _modules.Count;

    /// <inheritdoc/>
    public bool Contains(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _modules.ContainsKey(id);
    }

    /// <inheritdoc/>
    public IModule Get(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!_modules.TryGetValue(id, out var module))
        {
            throw new KeyNotFoundException(
                $"Module '{id}' is not registered.");
        }

        return module;
    }

    /// <inheritdoc/>
    public bool TryGet(string id, out IModule? module)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _modules.TryGetValue(id, out module);
    }

    /// <inheritdoc/>
    public void Register(IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var id = module.Metadata.Id;

        if (_modules.ContainsKey(id))
        {
            throw new InvalidOperationException(
                $"Module '{id}' is already registered.");
        }

        _modules.Add(id, module);
    }

    /// <inheritdoc/>
    public IEnumerator<IModule> GetEnumerator()
    {
        return _modules.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}