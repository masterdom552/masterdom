namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents the metadata describing a platform module.
/// </summary>
public sealed class ModuleDescriptor : IModuleMetadata
{
    /// <summary>
    /// Gets the unique, stable identifier of the module.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the technical name of the module.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the user-friendly display name of the module.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the module version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the module description.
    /// </summary>
    public required string Description { get; init; }
}