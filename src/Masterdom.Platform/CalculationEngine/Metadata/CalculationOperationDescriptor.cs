using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// Immutable metadata contract for a calculation operation.
/// </summary>
public sealed record CalculationOperationDescriptor : ICalculationOperationDescriptor
{
    private ImmutableArray<CalculationOperationCapabilityId> _dependencyCapabilityIds = ImmutableArray<CalculationOperationCapabilityId>.Empty;

    private ImmutableArray<string> _technicalTags = ImmutableArray<string>.Empty;

    private ImmutableArray<string> _mathematicalTags = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets or initializes the permanent descriptor identifier.
    /// </summary>
    public required CalculationOperationDescriptorId DescriptorId { get; init; }

    /// <summary>
    /// Gets or initializes the source type for diagnostics and governance.
    /// </summary>
    public required CalculationOperationDescriptorSourceType SourceType { get; init; }

    /// <summary>
    /// Gets or initializes the metadata schema version.
    /// </summary>
    public required string SchemaVersion { get; init; }

    /// <summary>
    /// Gets or initializes the operation name.
    /// </summary>
    public required string OperationName { get; init; }

    /// <summary>
    /// Gets or initializes the capability identifier.
    /// </summary>
    public required CalculationOperationCapabilityId CapabilityId { get; init; }

    /// <summary>
    /// Gets or initializes the operation version.
    /// </summary>
    public required CalculationOperationVersion OperationVersion { get; init; }

    /// <summary>
    /// Gets or initializes the operation description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets or initializes the primitive family.
    /// </summary>
    public required CalculationOperationPrimitiveFamily PrimitiveFamily { get; init; }

    /// <summary>
    /// Gets or initializes the capability category.
    /// </summary>
    public required CalculationOperationCapabilityCategory CapabilityCategory { get; init; }

    /// <summary>
    /// Gets or initializes the composition level.
    /// </summary>
    public required CalculationOperationCompositionLevel CompositionLevel { get; init; }

    /// <summary>
    /// Gets or initializes the operation category.
    /// </summary>
    public required CalculationOperationCategory OperationCategory { get; init; }

    /// <summary>
    /// Gets or initializes the execution classification.
    /// </summary>
    public required CalculationOperationExecutionClassification ExecutionClassification { get; init; }

    /// <summary>
    /// Gets or initializes the purity classification.
    /// </summary>
    public required CalculationOperationPurity Purity { get; init; }

    /// <summary>
    /// Gets or initializes the determinism classification.
    /// </summary>
    public required CalculationOperationDeterminism Determinism { get; init; }

    /// <summary>
    /// Gets or initializes the stability classification.
    /// </summary>
    public required CalculationOperationStability Stability { get; init; }

    /// <summary>
    /// Gets or initializes the compatibility classification.
    /// </summary>
    public required CalculationOperationCompatibilityStatus CompatibilityStatus { get; init; }

    /// <summary>
    /// Gets or initializes the time complexity metadata.
    /// </summary>
    public required string TimeComplexity { get; init; }

    /// <summary>
    /// Gets or initializes the space complexity metadata.
    /// </summary>
    public required string SpaceComplexity { get; init; }

    /// <summary>
    /// Gets or initializes the dependency capability identifiers.
    /// </summary>
    public IReadOnlyCollection<CalculationOperationCapabilityId> DependencyCapabilityIds
    {
        get => _dependencyCapabilityIds;
        init => _dependencyCapabilityIds = value is null ? ImmutableArray<CalculationOperationCapabilityId>.Empty : ImmutableArray.CreateRange(value);
    }

    /// <summary>
    /// Gets or initializes the technical tags.
    /// </summary>
    public IReadOnlyCollection<string> TechnicalTags
    {
        get => _technicalTags;
        init => _technicalTags = value is null ? ImmutableArray<string>.Empty : ImmutableArray.CreateRange(value);
    }

    /// <summary>
    /// Gets or initializes the mathematical tags.
    /// </summary>
    public IReadOnlyCollection<string> MathematicalTags
    {
        get => _mathematicalTags;
        init => _mathematicalTags = value is null ? ImmutableArray<string>.Empty : ImmutableArray.CreateRange(value);
    }
}
