namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class CalculationOperationMetadataIntegrityValidator
{
    private const string SupportedSchemaVersion = "1.0";

    public IReadOnlyList<string> Validate(IReadOnlyCollection<ICalculationOperationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var descriptorList = descriptors.ToArray();
        var warnings = new List<string>();

        ValidateFieldConventions(descriptorList, warnings);
        ValidateGraph(descriptorList);

        return warnings;
    }

    private static void ValidateFieldConventions(IReadOnlyList<ICalculationOperationDescriptor> descriptors, ICollection<string> warnings)
    {
        foreach (var descriptor in descriptors)
        {
            ValidateDescriptor(descriptor, warnings);
        }

        ValidateUniqueness(descriptors);
    }

    private static void ValidateDescriptor(ICalculationOperationDescriptor descriptor, ICollection<string> warnings)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!Enum.IsDefined(typeof(CalculationOperationDescriptorSourceType), descriptor.SourceType))
        {
            throw new CalculationOperationValidationException($"SourceType is invalid for '{descriptor.OperationName}'.");
        }

        if (!Enum.IsDefined(typeof(CalculationOperationCompatibilityStatus), descriptor.CompatibilityStatus))
        {
            throw new CalculationOperationValidationException($"CompatibilityStatus is invalid for '{descriptor.OperationName}'.");
        }

        if (!Enum.IsDefined(typeof(CalculationOperationCapabilityCategory), descriptor.CapabilityCategory))
        {
            throw new CalculationOperationValidationException($"CapabilityCategory is invalid for '{descriptor.OperationName}'.");
        }

        if (!IsCapabilityCategoryConsistent(descriptor))
        {
            throw new CalculationOperationValidationException($"CapabilityCategory '{descriptor.CapabilityCategory}' is not consistent with PrimitiveFamily '{descriptor.PrimitiveFamily}' for '{descriptor.OperationName}'.");
        }

        if (descriptor.CompatibilityStatus is CalculationOperationCompatibilityStatus.Deprecated)
        {
            warnings.Add($"Descriptor '{descriptor.OperationName}' is deprecated.");
        }

        if (descriptor.CompatibilityStatus is CalculationOperationCompatibilityStatus.Experimental && string.IsNullOrWhiteSpace(descriptor.SchemaVersion))
        {
            throw new CalculationOperationValidationException($"Experimental descriptor '{descriptor.OperationName}' must declare an explicit SchemaVersion.");
        }

        if (!string.Equals(descriptor.SchemaVersion, SupportedSchemaVersion, StringComparison.Ordinal))
        {
            throw new CalculationOperationValidationException($"SchemaVersion '{descriptor.SchemaVersion}' is not supported for '{descriptor.OperationName}'.");
        }

        if (!IsCapabilityIdValid(descriptor.CapabilityId.Value))
        {
            throw new CalculationOperationValidationException($"CapabilityId '{descriptor.CapabilityId.Value}' is invalid for '{descriptor.OperationName}'.");
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
            throw new CalculationOperationValidationException($"AbstractionType is required for '{descriptor.OperationName}'.");
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
            throw new CalculationOperationValidationException($"StabilityLevel is required for '{descriptor.OperationName}'.");
        }
    }

    private static void ValidateUniqueness(IReadOnlyList<ICalculationOperationDescriptor> descriptors)
    {
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

        var duplicateCapabilityIdsAcrossCategories = descriptors
            .GroupBy(descriptor => descriptor.CapabilityId.Value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(descriptor => descriptor.CapabilityCategory).Distinct().Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateCapabilityIdsAcrossCategories.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Duplicate calculation operation capability ids across categories were found: {string.Join(", ", duplicateCapabilityIdsAcrossCategories)}.");
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
    }

    private static void ValidateGraph(IReadOnlyList<ICalculationOperationDescriptor> descriptors)
    {
        var graph = CalculationOperationDependencyGraph.Build(descriptors);
        _ = graph.GetTopologicalOrdering();

        foreach (var descriptor in descriptors)
        {
            var duplicateDependencies = descriptor.DependencyCapabilityIds
                .GroupBy(dependency => dependency.Value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            if (duplicateDependencies.Length > 0)
            {
                throw new CalculationOperationValidationException(
                    $"Descriptor '{descriptor.OperationName}' contains duplicate dependency capability ids: {string.Join(", ", duplicateDependencies)}.");
            }

            if (descriptor.CompositionLevel is CalculationOperationCompositionLevel.Composite && descriptor.DependencyCapabilityIds.Count == 0)
            {
                throw new CalculationOperationValidationException($"Composite descriptor '{descriptor.OperationName}' must declare at least one dependency.");
            }

            foreach (var dependencyCapabilityId in descriptor.DependencyCapabilityIds)
            {
                if (string.Equals(dependencyCapabilityId.Value, descriptor.CapabilityId.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new CalculationOperationValidationException($"Descriptor '{descriptor.OperationName}' cannot depend on itself.");
                }

                if (!graph.TryResolveDescriptor(dependencyCapabilityId.Value, out var dependency))
                {
                    throw new CalculationOperationValidationException($"Descriptor '{descriptor.OperationName}' references missing dependency '{dependencyCapabilityId.Value}'.");
                }

                if (dependency.CompositionLevel is not CalculationOperationCompositionLevel.Primitive)
                {
                    throw new CalculationOperationValidationException($"Descriptor '{descriptor.OperationName}' must depend on primitive descriptors only.");
                }

                if (descriptor.CompositionLevel is CalculationOperationCompositionLevel.Composite && dependency.CompatibilityStatus is CalculationOperationCompatibilityStatus.Obsolete)
                {
                    throw new CalculationOperationValidationException($"Descriptor '{descriptor.OperationName}' cannot reference obsolete descriptor '{dependencyCapabilityId.Value}'.");
                }

                if (GetStabilityRank(descriptor.Stability) > GetStabilityRank(dependency.Stability))
                {
                    throw new CalculationOperationValidationException($"Descriptor '{descriptor.OperationName}' violates stability ordering with dependency '{dependencyCapabilityId.Value}'.");
                }
            }
        }
    }

    private static int GetStabilityRank(CalculationOperationStability stability)
    {
        return stability switch
        {
            CalculationOperationStability.Experimental => 1,
            CalculationOperationStability.Stable => 2,
            CalculationOperationStability.Fundamental => 3,
            _ => 0
        };
    }

    private static bool IsCapabilityCategoryConsistent(ICalculationOperationDescriptor descriptor)
    {
        return descriptor.CapabilityCategory switch
        {
            CalculationOperationCapabilityCategory.Aggregation => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Aggregation,
            CalculationOperationCapabilityCategory.Normalization => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Normalization,
            CalculationOperationCapabilityCategory.Interpolation => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Interpolation,
            CalculationOperationCapabilityCategory.Projection => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Projection,
            CalculationOperationCapabilityCategory.Statistics => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Statistics,
            CalculationOperationCapabilityCategory.Scoring => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Scoring,
            CalculationOperationCapabilityCategory.Ranking => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Ranking,
            CalculationOperationCapabilityCategory.Transformation => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Transformation,
            CalculationOperationCapabilityCategory.Validation => descriptor.PrimitiveFamily == CalculationOperationPrimitiveFamily.Validation,
            _ => false
        };
    }

    private static bool IsCapabilityIdValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var segments = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length < 2)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (segment.Length == 0 || !char.IsLower(segment[0]))
            {
                return false;
            }

            if (segment.Any(character => !char.IsLower(character) && !char.IsDigit(character) && character != '_'))
            {
                return false;
            }
        }

        return true;
    }
}
