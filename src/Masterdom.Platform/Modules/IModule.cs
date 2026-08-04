using Masterdom.Platform.Core;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Defines the contract for a platform module.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the module metadata.
    /// </summary>
    IModuleMetadata Metadata { get; }

    /// <summary>
    /// Initializes the module.
    /// </summary>
    /// <param name="context">The platform context.</param>
    void Initialize(IPlatformContext context);

    /// <summary>
    /// Shuts down the module.
    /// </summary>
    /// <param name="context">The platform context.</param>
    void Shutdown(IPlatformContext context);
}