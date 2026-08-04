using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// In-memory registry of calculation operation metadata.
/// </summary>
public sealed class CalculationOperationRegistry : ICalculationOperationRegistry
{
    private readonly ImmutableArray<ICalculationOperationDescriptor> _descriptors;
    private readonly Dictionary<string, ICalculationOperationDescriptor> _byDescriptorId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ICalculationOperationDescriptor> _byCapabilityId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ICalculationOperationDescriptor> _byOperationName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<CalculationOperationPrimitiveFamily, List<ICalculationOperationDescriptor>> _byPrimitiveFamily = new();
    private readonly Dictionary<CalculationOperationCapabilityCategory, List<ICalculationOperationDescriptor>> _byCapabilityCategory = new();
    private readonly Dictionary<CalculationOperationCompositionLevel, List<ICalculationOperationDescriptor>> _byCompositionLevel = new();
    private readonly Dictionary<CalculationOperationCompatibilityStatus, List<ICalculationOperationDescriptor>> _byCompatibilityStatus = new();

    public CalculationOperationRegistry()
        : this(new CalculationOperationDescriptorProvider().GetDescriptors())
    {
    }

    internal CalculationOperationRegistry(IEnumerable<ICalculationOperationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var descriptorList = descriptors
            .OrderBy(descriptor => descriptor.CapabilityId.Value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(descriptor => descriptor.DescriptorId.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var descriptor in descriptorList)
        {
            _byDescriptorId[descriptor.DescriptorId.Value] = descriptor;
            _byCapabilityId[descriptor.CapabilityId.Value] = descriptor;
            _byOperationName[descriptor.OperationName] = descriptor;
            GetOrCreateBucket(_byPrimitiveFamily, descriptor.PrimitiveFamily).Add(descriptor);
            GetOrCreateBucket(_byCapabilityCategory, descriptor.CapabilityCategory).Add(descriptor);
            GetOrCreateBucket(_byCompositionLevel, descriptor.CompositionLevel).Add(descriptor);
            GetOrCreateBucket(_byCompatibilityStatus, descriptor.CompatibilityStatus).Add(descriptor);
        }

        _descriptors = descriptorList.ToImmutableArray();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> GetAll()
    {
        return _descriptors;
    }

    public ICalculationOperationDescriptor ResolveByDescriptorId(CalculationOperationDescriptorId descriptorId)
    {
        ArgumentNullException.ThrowIfNull(descriptorId);

        if (_byDescriptorId.TryGetValue(descriptorId.Value, out var descriptor))
        {
            return descriptor;
        }

        throw new CalculationOperationValidationException($"Calculation operation descriptor '{descriptorId.Value}' was not found.");
    }

    public ICalculationOperationDescriptor ResolveByCapabilityId(CalculationOperationCapabilityId capabilityId)
    {
        ArgumentNullException.ThrowIfNull(capabilityId);

        if (_byCapabilityId.TryGetValue(capabilityId.Value, out var descriptor))
        {
            return descriptor;
        }

        throw new CalculationOperationValidationException($"Calculation operation capability '{capabilityId.Value}' was not found.");
    }

    public ICalculationOperationDescriptor ResolveByOperationName(string operationName)
    {
        if (string.IsNullOrWhiteSpace(operationName))
        {
            throw new CalculationOperationValidationException("OperationName is required.");
        }

        if (_byOperationName.TryGetValue(operationName, out var descriptor))
        {
            return descriptor;
        }

        throw new CalculationOperationValidationException($"Calculation operation '{operationName}' was not found.");
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByPrimitiveFamily(CalculationOperationPrimitiveFamily primitiveFamily)
    {
        if (primitiveFamily is CalculationOperationPrimitiveFamily.Unspecified)
        {
            throw new CalculationOperationValidationException("PrimitiveFamily is required.");
        }

        return _byPrimitiveFamily.TryGetValue(primitiveFamily, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> GetByCapabilityCategory(CalculationOperationCapabilityCategory capabilityCategory)
    {
        if (capabilityCategory is CalculationOperationCapabilityCategory.Unspecified)
        {
            throw new CalculationOperationValidationException("CapabilityCategory is required.");
        }

        return _byCapabilityCategory.TryGetValue(capabilityCategory, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompositionLevel(CalculationOperationCompositionLevel compositionLevel)
    {
        if (compositionLevel is CalculationOperationCompositionLevel.Unspecified)
        {
            throw new CalculationOperationValidationException("CompositionLevel is required.");
        }

        return _byCompositionLevel.TryGetValue(compositionLevel, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus compatibilityStatus)
    {
        if (compatibilityStatus is CalculationOperationCompatibilityStatus.Unspecified)
        {
            throw new CalculationOperationValidationException("CompatibilityStatus is required.");
        }

        return _byCompatibilityStatus.TryGetValue(compatibilityStatus, out var descriptors)
            ? new ReadOnlyCollection<ICalculationOperationDescriptor>(descriptors.ToArray())
            : Array.Empty<ICalculationOperationDescriptor>();
    }

    private static List<ICalculationOperationDescriptor> GetOrCreateBucket(
        IDictionary<CalculationOperationPrimitiveFamily, List<ICalculationOperationDescriptor>> index,
        CalculationOperationPrimitiveFamily key)
    {
        if (!index.TryGetValue(key, out var bucket))
        {
            bucket = [];
            index[key] = bucket;
        }

        return bucket;
    }

    private static List<ICalculationOperationDescriptor> GetOrCreateBucket(
        IDictionary<CalculationOperationCapabilityCategory, List<ICalculationOperationDescriptor>> index,
        CalculationOperationCapabilityCategory key)
    {
        if (!index.TryGetValue(key, out var bucket))
        {
            bucket = [];
            index[key] = bucket;
        }

        return bucket;
    }

    private static List<ICalculationOperationDescriptor> GetOrCreateBucket(
        IDictionary<CalculationOperationCompositionLevel, List<ICalculationOperationDescriptor>> index,
        CalculationOperationCompositionLevel key)
    {
        if (!index.TryGetValue(key, out var bucket))
        {
            bucket = [];
            index[key] = bucket;
        }

        return bucket;
    }

    private static List<ICalculationOperationDescriptor> GetOrCreateBucket(
        IDictionary<CalculationOperationCompatibilityStatus, List<ICalculationOperationDescriptor>> index,
        CalculationOperationCompatibilityStatus key)
    {
        if (!index.TryGetValue(key, out var bucket))
        {
            bucket = [];
            index[key] = bucket;
        }

        return bucket;
    }
}
