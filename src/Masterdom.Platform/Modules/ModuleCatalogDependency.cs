using System;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents a dependency declared by a module catalog entry.
/// </summary>
public sealed class ModuleCatalogDependency
{
    /// <summary>
    /// Gets the required module identifier.
    /// </summary>
    public required string ModuleId { get; init; }

    /// <summary>
    /// Gets the required module version. When null, any version is accepted.
    /// </summary>
    public string? RequiredVersion { get; init; }
}
