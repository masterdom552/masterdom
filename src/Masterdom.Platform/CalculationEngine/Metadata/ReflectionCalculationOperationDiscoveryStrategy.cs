using System.Collections.Immutable;
using System.Reflection;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class ReflectionCalculationOperationDiscoveryStrategy : ICalculationOperationDiscoveryStrategy
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        var descriptorSourceTypes = typeof(ReflectionCalculationOperationDiscoveryStrategy)
            .Assembly
            .DefinedTypes
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                typeof(ICalculationOperationDescriptorSource).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        var descriptors = descriptorSourceTypes
            .Select(type => (ICalculationOperationDescriptorSource)Activator.CreateInstance(type.AsType())!)
            .SelectMany(source => source.GetDescriptors())
            .ToArray();

        return ImmutableArray.CreateRange(descriptors);
    }
}
