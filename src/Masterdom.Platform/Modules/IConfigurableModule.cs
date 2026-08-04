using Masterdom.Platform.Services;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Enables a module to register runtime services before initialization.
/// </summary>
public interface IConfigurableModule
{
    /// <summary>
    /// Registers module services.
    /// </summary>
    void ConfigureServices(IPlatformServiceRegistry services);
}
