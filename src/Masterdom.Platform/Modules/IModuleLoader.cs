namespace Masterdom.Platform.Modules;

/// <summary>
/// Defines the contract for loading modules into the platform.
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Loads a module into the specified registry.
    /// </summary>
    /// <param name="module">The module to load.</param>
    /// <param name="registry">The target registry.</param>
    void Load(IModule module, IModuleRegistry registry);
}
