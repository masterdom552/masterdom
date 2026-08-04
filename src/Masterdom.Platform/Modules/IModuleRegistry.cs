namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the writable platform module registry.
/// </summary>
public interface IModuleRegistry : IModuleCatalog
{
    /// <summary>
    /// Registers a module.
    /// </summary>
    void Register(IModule module);
}