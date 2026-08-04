using System.Collections.Generic;

namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// Represents metadata for a calculation operation.
/// </summary>
public interface ICalculationOperationDescriptor
{
    /// <summary>
    /// Gets the permanent descriptor identifier.
    /// </summary>
    CalculationOperationDescriptorId DescriptorId { get; }

    /// <summary>
    /// Gets the descriptor source type.
    /// </summary>
    CalculationOperationDescriptorSourceType SourceType { get; }

    /// <summary>
    /// Gets the metadata schema version.
    /// </summary>
    string SchemaVersion { get; }

    /// <summary>
    /// Gets the operation name.
    /// </summary>
    string OperationName { get; }

    /// <summary>
    /// Gets the capability identifier.
    /// </summary>
    CalculationOperationCapabilityId CapabilityId { get; }

    /// <summary>
    /// Gets the operation version.
    /// </summary>
    CalculationOperationVersion OperationVersion { get; }

    /// <summary>
    /// Gets the operation description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the primitive family.
    /// </summary>
    CalculationOperationPrimitiveFamily PrimitiveFamily { get; }

    /// <summary>
    /// Gets the capability category.
    /// </summary>
    CalculationOperationCapabilityCategory CapabilityCategory { get; }

    /// <summary>
    /// Gets the composition level.
    /// </summary>
    CalculationOperationCompositionLevel CompositionLevel { get; }

    /// <summary>
    /// Gets the operation category.
    /// </summary>
    CalculationOperationCategory OperationCategory { get; }

    /// <summary>
    /// Gets the execution classification.
    /// </summary>
    CalculationOperationExecutionClassification ExecutionClassification { get; }

    /// <summary>
    /// Gets the purity classification.
    /// </summary>
    CalculationOperationPurity Purity { get; }

    /// <summary>
    /// Gets the determinism classification.
    /// </summary>
    CalculationOperationDeterminism Determinism { get; }

    /// <summary>
    /// Gets the stability classification.
    /// </summary>
    CalculationOperationStability Stability { get; }

    /// <summary>
    /// Gets the compatibility classification.
    /// </summary>
    CalculationOperationCompatibilityStatus CompatibilityStatus { get; }

    /// <summary>
    /// Gets the time complexity metadata.
    /// </summary>
    string TimeComplexity { get; }

    /// <summary>
    /// Gets the space complexity metadata.
    /// </summary>
    string SpaceComplexity { get; }

    /// <summary>
    /// Gets the dependency capability identifiers.
    /// </summary>
    IReadOnlyCollection<CalculationOperationCapabilityId> DependencyCapabilityIds { get; }

    /// <summary>
    /// Gets the technical tags.
    /// </summary>
    IReadOnlyCollection<string> TechnicalTags { get; }

    /// <summary>
    /// Gets the mathematical tags.
    /// </summary>
    IReadOnlyCollection<string> MathematicalTags { get; }
}
