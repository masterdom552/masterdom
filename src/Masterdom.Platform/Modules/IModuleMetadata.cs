namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the immutable metadata that describes a platform module.
/// </summary>
public interface IModuleMetadata
{
    /// <summary>
    /// Gets the unique, stable identifier of the module.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the technical name of the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the user-friendly display name of the module.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the module version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the module description.
    /// </summary>
    string Description { get; }
}