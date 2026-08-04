using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Represents an authoritative catalog entry for a platform module.
/// </summary>
public sealed class ModuleCatalogEntry
{
    /// <summary>
    /// Gets the module identifier.
    /// </summary>
    public required string ModuleId { get; init; }

    /// <summary>
    /// Gets the module instance.
    /// </summary>
    public required IModule Module { get; init; }

    /// <summary>
    /// Gets the module version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets module dependencies.
    /// </summary>
    public IReadOnlyList<ModuleCatalogDependency> Dependencies { get; init; } =
        Array.Empty<ModuleCatalogDependency>();

    /// <summary>
    /// Gets service types that must exist before module initialization.
    /// </summary>
    public IReadOnlyList<Type> RequiredServices { get; init; } =
        Array.Empty<Type>();

    /// <summary>
    /// Gets service types that are optional for module initialization.
    /// </summary>
    public IReadOnlyList<Type> OptionalServices { get; init; } =
        Array.Empty<Type>();

    /// <summary>
    /// Gets the startup order priority. Lower values start earlier.
    /// </summary>
    public int StartupOrder { get; init; }

    /// <summary>
    /// Gets logical health check identifiers owned by this module.
    /// </summary>
    public IReadOnlyList<string> HealthChecks { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Gets capability identifiers exposed by this module.
    /// </summary>
    public IReadOnlyList<string> Capabilities { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Gets module configuration values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Configuration { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
