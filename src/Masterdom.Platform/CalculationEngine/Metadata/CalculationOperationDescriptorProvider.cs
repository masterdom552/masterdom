using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class CalculationOperationDescriptorProvider : ICalculationOperationDescriptorProvider
{
    private readonly ICompositeCalculationOperationDiscoveryStrategy _discoveryStrategy;
    private readonly CalculationOperationMetadataIntegrityValidator _integrityValidator;
    private readonly Lazy<ImmutableArray<ICalculationOperationDescriptor>> _descriptors;

    public CalculationOperationDescriptorProvider()
        : this(new CompositeCalculationOperationDiscoveryStrategy())
    {
    }

    internal CalculationOperationDescriptorProvider(ICompositeCalculationOperationDiscoveryStrategy discoveryStrategy)
        : this(discoveryStrategy, new CalculationOperationMetadataIntegrityValidator())
    {
    }

    internal CalculationOperationDescriptorProvider(
        ICompositeCalculationOperationDiscoveryStrategy discoveryStrategy,
        CalculationOperationMetadataIntegrityValidator integrityValidator)
    {
        ArgumentNullException.ThrowIfNull(discoveryStrategy);
        ArgumentNullException.ThrowIfNull(integrityValidator);

        _discoveryStrategy = discoveryStrategy;
        _integrityValidator = integrityValidator;
        _descriptors = new Lazy<ImmutableArray<ICalculationOperationDescriptor>>(DiscoverDescriptors);
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return _descriptors.Value;
    }

    private ImmutableArray<ICalculationOperationDescriptor> DiscoverDescriptors()
    {
        var descriptors = _discoveryStrategy.GetDescriptors();

        ValidateDescriptors(descriptors);
        _integrityValidator.Validate(descriptors);

        return ImmutableArray.CreateRange(descriptors);
    }

    private static void ValidateDescriptors(IReadOnlyCollection<ICalculationOperationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var duplicateDescriptorIds = descriptors
            .GroupBy(descriptor => descriptor.DescriptorId.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateDescriptorIds.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Duplicate calculation operation descriptor ids were found: {string.Join(", ", duplicateDescriptorIds)}.");
        }

        var duplicateCapabilityIds = descriptors
            .GroupBy(descriptor => descriptor.CapabilityId.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCapabilityIds.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Duplicate calculation operation capability ids were found: {string.Join(", ", duplicateCapabilityIds)}.");
        }

        var duplicateOperationNames = descriptors
            .GroupBy(descriptor => descriptor.OperationName, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateOperationNames.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Duplicate calculation operation names were found: {string.Join(", ", duplicateOperationNames)}.");
        }

        foreach (var descriptor in descriptors)
        {
            ValidateDescriptor(descriptor);
        }
    }

    private static void ValidateDescriptor(ICalculationOperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (descriptor.DescriptorId.IsDefault)
        {
            throw new CalculationOperationValidationException("DescriptorId is required.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.DescriptorId.Value))
        {
            throw new CalculationOperationValidationException("DescriptorId is required.");
        }

        if (descriptor.CapabilityId.IsDefault)
        {
            throw new CalculationOperationValidationException("CapabilityId is required.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.CapabilityId.Value))
        {
            throw new CalculationOperationValidationException("CapabilityId is required.");
        }

        if (descriptor.OperationVersion.IsDefault)
        {
            throw new CalculationOperationValidationException($"OperationVersion is required for '{descriptor.OperationName}'.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.OperationVersion.Value))
        {
            throw new CalculationOperationValidationException($"OperationVersion is required for '{descriptor.OperationName}'.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.OperationName))
        {
            throw new CalculationOperationValidationException("OperationName is required.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.Description))
        {
            throw new CalculationOperationValidationException($"Description is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.PrimitiveFamily is CalculationOperationPrimitiveFamily.Unspecified)
        {
            throw new CalculationOperationValidationException($"PrimitiveFamily is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.CompositionLevel is CalculationOperationCompositionLevel.Unspecified)
        {
            throw new CalculationOperationValidationException($"CompositionLevel is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.OperationCategory is CalculationOperationCategory.Unspecified)
        {
            throw new CalculationOperationValidationException($"OperationCategory is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.ExecutionClassification is CalculationOperationExecutionClassification.Unspecified)
        {
            throw new CalculationOperationValidationException($"ExecutionClassification is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.Purity is CalculationOperationPurity.Unspecified)
        {
            throw new CalculationOperationValidationException($"Purity is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.Determinism is CalculationOperationDeterminism.Unspecified)
        {
            throw new CalculationOperationValidationException($"Determinism is required for '{descriptor.OperationName}'.");
        }

        if (descriptor.Stability is CalculationOperationStability.Unspecified)
        {
            throw new CalculationOperationValidationException($"Stability is required for '{descriptor.OperationName}'.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.TimeComplexity))
        {
            throw new CalculationOperationValidationException($"TimeComplexity is required for '{descriptor.OperationName}'.");
        }

        if (string.IsNullOrWhiteSpace(descriptor.SpaceComplexity))
        {
            throw new CalculationOperationValidationException($"SpaceComplexity is required for '{descriptor.OperationName}'.");
        }

        foreach (var tag in descriptor.TechnicalTags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new CalculationOperationValidationException($"TechnicalTags for '{descriptor.OperationName}' must not contain empty values.");
            }
        }

        foreach (var tag in descriptor.MathematicalTags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new CalculationOperationValidationException($"MathematicalTags for '{descriptor.OperationName}' must not contain empty values.");
            }
        }
    }
}
