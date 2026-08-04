using System;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Default implementation of <see cref="IModuleLoader"/>.
/// </summary>
public sealed class ModuleLoader : IModuleLoader
{
    /// <inheritdoc />
    public void Load(IModule module, IModuleRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(registry);

        ModuleValidator.Validate(module);

        registry.Register(module);
    }
}
