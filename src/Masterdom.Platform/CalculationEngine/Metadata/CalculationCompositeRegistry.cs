using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal interface ICalculationCompositeRegistry
{
    IReadOnlyCollection<ICalculationOperationDescriptor> DiscoverComposites();

    ICalculationOperationDescriptor ResolveByCapabilityId(CalculationOperationCapabilityId capabilityId);

    ICalculationOperationDescriptor ResolveByDescriptorId(CalculationOperationDescriptorId descriptorId);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByFamily(CalculationOperationPrimitiveFamily family);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompatibility(CalculationOperationCompatibilityStatus compatibilityStatus);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByStability(CalculationOperationStability stability);
}

internal sealed class CalculationCompositeRegistry : ICalculationCompositeRegistry
{
    private readonly CalculationCompositeDependencyGraphValidator _dependencyGraphValidator;
    private readonly ImmutableArray<ICalculationOperationDescriptor> _composites;
    private readonly Dictionary<string, ICalculationOperationDescriptor> _byCapabilityId;
    private readonly Dictionary<string, ICalculationOperationDescriptor> _byDescriptorId;
    private readonly Dictionary<CalculationOperationPrimitiveFamily, List<ICalculationOperationDescriptor>> _byFamily;
    private readonly Dictionary<CalculationOperationCompatibilityStatus, List<ICalculationOperationDescriptor>> _byCompatibility;
    private readonly Dictionary<CalculationOperationStability, List<ICalculationOperationDescriptor>> _byStability;

    public CalculationCompositeRegistry()
        : this(new CalculationOperationRegistry())
    {
    }

    internal CalculationCompositeRegistry(ICalculationOperationRegistry operationRegistry)
        : this(operationRegistry, new CalculationCompositeDependencyGraphValidator())
    {
    }

    internal CalculationCompositeRegistry(
        ICalculationOperationRegistry operationRegistry,
        CalculationCompositeDependencyGraphValidator dependencyGraphValidator)
    {
        ArgumentNullException.ThrowIfNull(operationRegistry);
        ArgumentNullException.ThrowIfNull(dependencyGraphValidator);

        _dependencyGraphValidator = dependencyGraphValidator;

        var allDescriptors = operationRegistry.GetAll();
        _dependencyGraphValidator.Validate(allDescriptors);

        _composites = allDescriptors
            .Where(descriptor => descriptor.CompositionLevel == CalculationOperationCompositionLevel.Composite)
            .ToImmutableArray();

        _byCapabilityId = _composites.ToDictionary(
            descriptor => descriptor.CapabilityId.Value,
            descriptor => descriptor,
            StringComparer.OrdinalIgnoreCase);

        _byDescriptorId = _composites.ToDictionary(
            descriptor => descriptor.DescriptorId.Value,
            descriptor => descriptor,
            StringComparer.OrdinalIgnoreCase);

        _byFamily = BuildIndex(_composites, descriptor => descriptor.PrimitiveFamily);
        _byCompatibility = BuildIndex(_composites, descriptor => descriptor.CompatibilityStatus);
        _byStability = BuildIndex(_composites, descriptor => descriptor.Stability);
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> DiscoverComposites()
    {
        return _composites;
    }

    public ICalculationOperationDescriptor ResolveByCapabilityId(CalculationOperationCapabilityId capabilityId)
    {
        ArgumentNullException.ThrowIfNull(capabilityId);

        if (_byCapabilityId.TryGetValue(capabilityId.Value, out var descriptor))
        {
            return descriptor;
        }

        throw new CalculationOperationValidationException(
            $"Composite capability '{capabilityId.Value}' was not found.");
    }

    public ICalculationOperationDescriptor ResolveByDescriptorId(CalculationOperationDescriptorId descriptorId)
    {
        ArgumentNullException.ThrowIfNull(descriptorId);

        if (_byDescriptorId.TryGetValue(descriptorId.Value, out var descriptor))
        {
            return descriptor;
        }

        throw new CalculationOperationValidationException(
            $"Composite descriptor '{descriptorId.Value}' was not found.");
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByFamily(CalculationOperationPrimitiveFamily family)
    {
        if (family == CalculationOperationPrimitiveFamily.Unspecified)
        {
            throw new CalculationOperationValidationException("PrimitiveFamily is required.");
        }

        return _byFamily.TryGetValue(family, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompatibility(CalculationOperationCompatibilityStatus compatibilityStatus)
    {
        if (compatibilityStatus == CalculationOperationCompatibilityStatus.Unspecified)
        {
            throw new CalculationOperationValidationException("CompatibilityStatus is required.");
        }

        return _byCompatibility.TryGetValue(compatibilityStatus, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByStability(CalculationOperationStability stability)
    {
        if (stability == CalculationOperationStability.Unspecified)
        {
            throw new CalculationOperationValidationException("Stability is required.");
        }

        return _byStability.TryGetValue(stability, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    private static Dictionary<TKey, List<ICalculationOperationDescriptor>> BuildIndex<TKey>(
        IEnumerable<ICalculationOperationDescriptor> descriptors,
        Func<ICalculationOperationDescriptor, TKey> keySelector)
        where TKey : notnull
    {
        var index = new Dictionary<TKey, List<ICalculationOperationDescriptor>>();

        foreach (var descriptor in descriptors)
        {
            var key = keySelector(descriptor);
            if (!index.TryGetValue(key, out var values))
            {
                values = [];
                index[key] = values;
            }

            values.Add(descriptor);
        }

        return index;
    }
}
