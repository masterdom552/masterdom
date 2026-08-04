using System;
using Masterdom.Platform.Core;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Provides a base implementation of <see cref="IModule"/>.
/// </summary>
public abstract class ModuleBase : IModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleBase"/> class.
    /// </summary>
    /// <param name="metadata">The module metadata.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    protected ModuleBase(IModuleMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <inheritdoc/>
    public IModuleMetadata Metadata { get; }

    /// <inheritdoc/>
    public virtual void Initialize(IPlatformContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    /// <inheritdoc/>
    public virtual void Shutdown(IPlatformContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }
}