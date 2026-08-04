namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the base class for built-in platform modules.
/// </summary>
public abstract class PlatformModule : ModuleBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformModule"/> class.
    /// </summary>
    /// <param name="metadata">The module metadata.</param>
    protected PlatformModule(IModuleMetadata metadata)
        : base(metadata)
    {
    }
}