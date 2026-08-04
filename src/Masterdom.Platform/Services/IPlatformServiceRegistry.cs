using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Services;

/// <summary>
/// Provides runtime service registration and retrieval for the platform.
/// </summary>
public interface IPlatformServiceRegistry
{
    /// <summary>
    /// Registers a singleton service instance.
    /// </summary>
    void AddSingleton<TService>(TService instance)
        where TService : class;

    /// <summary>
    /// Registers a singleton service instance.
    /// </summary>
    void AddSingleton(Type serviceType, object instance);

    /// <summary>
    /// Determines whether a service has been registered.
    /// </summary>
    bool Contains(Type serviceType);

    /// <summary>
    /// Gets a required service instance.
    /// </summary>
    TService GetRequired<TService>()
        where TService : class;

    /// <summary>
    /// Gets a required service instance.
    /// </summary>
    object GetRequired(Type serviceType);

    /// <summary>
    /// Attempts to get a registered service.
    /// </summary>
    bool TryGet<TService>(out TService? service)
        where TService : class;

    /// <summary>
    /// Gets the current service registrations snapshot.
    /// </summary>
    IReadOnlyDictionary<Type, object> Snapshot();
}
