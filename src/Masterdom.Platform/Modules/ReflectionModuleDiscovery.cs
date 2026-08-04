using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Discovers modules by scanning assemblies for <see cref="IModule"/> implementations.
/// </summary>
public sealed class ReflectionModuleDiscovery : IModuleDiscovery
{
    /// <inheritdoc />
    public IReadOnlyList<IModule> Discover(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var discovered = new List<IModule>();

        foreach (var assembly in assemblies.Where(a => a is not null).Distinct())
        {
            var candidates = assembly
                .DefinedTypes
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    typeof(IModule).IsAssignableFrom(type.AsType()))
                .ToList();

            foreach (var candidate in candidates)
            {
                var ctor = candidate.GetConstructor(Type.EmptyTypes);

                if (ctor is null)
                {
                    continue;
                }

                try
                {
                    var instance = (IModule)Activator.CreateInstance(candidate.AsType())!;
                    discovered.Add(instance);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to instantiate module '{candidate.FullName}'.",
                        ex);
                }
            }
        }

        return discovered;
    }
}
