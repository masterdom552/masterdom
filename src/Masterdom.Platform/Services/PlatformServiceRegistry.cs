using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Services;

/// <summary>
/// Default in-memory implementation of <see cref="IPlatformServiceRegistry"/>.
/// </summary>
public sealed class PlatformServiceRegistry : IPlatformServiceRegistry
{
    private readonly Dictionary<Type, object> _services = new();

    /// <inheritdoc />
    public void AddSingleton<TService>(TService instance)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        AddSingleton(typeof(TService), instance);
    }

    /// <inheritdoc />
    public void AddSingleton(Type serviceType, object instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(instance);

        if (!serviceType.IsInstanceOfType(instance))
        {
            throw new ArgumentException(
                $"Instance type '{instance.GetType().FullName}' " +
                $"does not implement '{serviceType.FullName}'.",
                nameof(instance));
        }

        if (_services.ContainsKey(serviceType))
        {
            throw new InvalidOperationException(
                $"Service '{serviceType.FullName}' is already registered.");
        }

        _services.Add(serviceType, instance);
    }

    /// <inheritdoc />
    public bool Contains(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return _services.ContainsKey(serviceType);
    }

    /// <inheritdoc />
    public TService GetRequired<TService>()
        where TService : class
    {
        return (TService)GetRequired(typeof(TService));
    }

    /// <inheritdoc />
    public object GetRequired(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (!_services.TryGetValue(serviceType, out var service))
        {
            throw new KeyNotFoundException(
                $"Service '{serviceType.FullName}' was not found.");
        }

        return service;
    }

    /// <inheritdoc />
    public bool TryGet<TService>(out TService? service)
        where TService : class
    {
        var found = _services.TryGetValue(typeof(TService), out var value);

        service = found ? (TService)value : null;

        return found;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Type, object> Snapshot()
    {
        return _services;
    }
}
