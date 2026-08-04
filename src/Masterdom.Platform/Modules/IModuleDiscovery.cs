using System.Collections.Generic;
using System.Reflection;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Discovers modules from assemblies.
/// </summary>
public interface IModuleDiscovery
{
    /// <summary>
    /// Discovers modules from the specified assemblies.
    /// </summary>
    IReadOnlyList<IModule> Discover(IEnumerable<Assembly> assemblies);
}
