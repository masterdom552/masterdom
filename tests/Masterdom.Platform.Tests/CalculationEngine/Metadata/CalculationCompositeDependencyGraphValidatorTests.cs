using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.Tests.CalculationEngine.Metadata;

public sealed class CalculationCompositeDependencyGraphValidatorTests
{
    [Fact]
    public void Validator_ShouldAccept_FrozenCompositeGraph()
    {
        var descriptors = new CalculationOperationRegistry().GetAll();
        var validator = new CalculationCompositeDependencyGraphValidator();

        validator.Validate(descriptors);
    }

    [Fact]
    public void Validator_ShouldReject_MissingPrimitiveDependency()
    {
        var validator = new CalculationCompositeDependencyGraphValidator();

        var descriptors = new ICalculationOperationDescriptor[]
        {
            BuildPrimitive("ce-op-test-0001", "aggregation.mean"),
            BuildComposite(
                "ce-op-test-0002",
                "estimation.test",
                ["aggregation.mean", "normalization.missing"])
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate(descriptors));
    }

    [Fact]
    public void Validator_ShouldReject_DuplicatePrimitiveDependency()
    {
        var validator = new CalculationCompositeDependencyGraphValidator();

        var descriptors = new ICalculationOperationDescriptor[]
        {
            BuildPrimitive("ce-op-test-0001", "aggregation.mean"),
            BuildComposite(
                "ce-op-test-0002",
                "estimation.test",
                ["aggregation.mean", "aggregation.mean"])
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate(descriptors));
    }

    [Fact]
    public void Validator_ShouldReject_CompositeDependency_WhenNotAllowed()
    {
        var validator = new CalculationCompositeDependencyGraphValidator();

        var descriptors = new ICalculationOperationDescriptor[]
        {
            BuildPrimitive("ce-op-test-0001", "aggregation.mean"),
            BuildComposite("ce-op-test-0002", "estimation.base", ["aggregation.mean"]),
            BuildComposite("ce-op-test-0003", "estimation.derived", ["estimation.base"])
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate(descriptors));
    }

    [Fact]
    public void Validator_ShouldReject_CompositeDependencyCycle_WhenExplicitlyAllowed()
    {
        var validator = new CalculationCompositeDependencyGraphValidator();

        var descriptors = new ICalculationOperationDescriptor[]
        {
            BuildPrimitive("ce-op-test-0001", "aggregation.mean"),
            BuildComposite("ce-op-test-0002", "estimation.base", ["aggregation.mean", "estimation.derived"]),
            BuildComposite("ce-op-test-0003", "estimation.derived", ["aggregation.mean", "estimation.base"])
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate(descriptors, allowCompositeDependencies: true));
    }

    [Fact]
    public void Validator_ShouldAccept_RandomizedDescriptorOrdering()
    {
        var validator = new CalculationCompositeDependencyGraphValidator();

        var descriptorProvider = new CalculationOperationDescriptorProvider();
        var randomized = descriptorProvider
            .GetDescriptors()
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();

        validator.Validate(randomized);
    }

    private static CalculationOperationDescriptor BuildPrimitive(string descriptorId, string capabilityId)
    {
        var family = ResolveFamily(capabilityId);

        return new CalculationOperationDescriptor
        {
            DescriptorId = CalculationOperationDescriptorId.Create(descriptorId),
            SourceType = CalculationOperationDescriptorSourceType.Test,
            SchemaVersion = "1.0",
            OperationName = $"Primitive {capabilityId}",
            CapabilityId = CalculationOperationCapabilityId.Create(capabilityId),
            OperationVersion = CalculationOperationVersion.Create("1.0.0"),
            Description = "Test primitive.",
            PrimitiveFamily = family,
            CapabilityCategory = ResolveCategory(family),
            CompositionLevel = CalculationOperationCompositionLevel.Primitive,
            OperationCategory = CalculationOperationCategory.Primitive,
            ExecutionClassification = CalculationOperationExecutionClassification.Primitive,
            Purity = CalculationOperationPurity.Pure,
            Determinism = CalculationOperationDeterminism.Deterministic,
            Stability = CalculationOperationStability.Fundamental,
            CompatibilityStatus = CalculationOperationCompatibilityStatus.Supported,
            TimeComplexity = "O(1)",
            SpaceComplexity = "O(1)",
            DependencyCapabilityIds = [],
            TechnicalTags = ["primitive"],
            MathematicalTags = ["test"]
        };
    }

    private static CalculationOperationDescriptor BuildComposite(
        string descriptorId,
        string capabilityId,
        IReadOnlyCollection<string> dependencies)
    {
        var family = ResolveFamily(capabilityId);

        return new CalculationOperationDescriptor
        {
            DescriptorId = CalculationOperationDescriptorId.Create(descriptorId),
            SourceType = CalculationOperationDescriptorSourceType.Test,
            SchemaVersion = "1.0",
            OperationName = $"Composite {capabilityId}",
            CapabilityId = CalculationOperationCapabilityId.Create(capabilityId),
            OperationVersion = CalculationOperationVersion.Create("1.0.0"),
            Description = "Test composite.",
            PrimitiveFamily = family,
            CapabilityCategory = ResolveCategory(family),
            CompositionLevel = CalculationOperationCompositionLevel.Composite,
            OperationCategory = CalculationOperationCategory.Composite,
            ExecutionClassification = CalculationOperationExecutionClassification.Composite,
            Purity = CalculationOperationPurity.Contextual,
            Determinism = CalculationOperationDeterminism.Deterministic,
            Stability = CalculationOperationStability.Stable,
            CompatibilityStatus = CalculationOperationCompatibilityStatus.Supported,
            TimeComplexity = "O(1)",
            SpaceComplexity = "O(1)",
            DependencyCapabilityIds = dependencies.Select(CalculationOperationCapabilityId.Create).ToArray(),
            TechnicalTags = ["composite"],
            MathematicalTags = ["test"]
        };
    }

    private static CalculationOperationPrimitiveFamily ResolveFamily(string capabilityId)
    {
        return capabilityId.Split('.', StringSplitOptions.RemoveEmptyEntries)[0] switch
        {
            "aggregation" => CalculationOperationPrimitiveFamily.Aggregation,
            "normalization" => CalculationOperationPrimitiveFamily.Normalization,
            "interpolation" => CalculationOperationPrimitiveFamily.Interpolation,
            "projection" => CalculationOperationPrimitiveFamily.Projection,
            "statistics" => CalculationOperationPrimitiveFamily.Statistics,
            "scoring" => CalculationOperationPrimitiveFamily.Scoring,
            "ranking" => CalculationOperationPrimitiveFamily.Ranking,
            "transformation" => CalculationOperationPrimitiveFamily.Transformation,
            "validation" => CalculationOperationPrimitiveFamily.Validation,
            "estimation" => CalculationOperationPrimitiveFamily.Aggregation,
            "forecast" => CalculationOperationPrimitiveFamily.Projection,
            _ => CalculationOperationPrimitiveFamily.Validation
        };
    }

    private static CalculationOperationCapabilityCategory ResolveCategory(CalculationOperationPrimitiveFamily family)
    {
        return family switch
        {
            CalculationOperationPrimitiveFamily.Aggregation => CalculationOperationCapabilityCategory.Aggregation,
            CalculationOperationPrimitiveFamily.Normalization => CalculationOperationCapabilityCategory.Normalization,
            CalculationOperationPrimitiveFamily.Interpolation => CalculationOperationCapabilityCategory.Interpolation,
            CalculationOperationPrimitiveFamily.Projection => CalculationOperationCapabilityCategory.Projection,
            CalculationOperationPrimitiveFamily.Statistics => CalculationOperationCapabilityCategory.Statistics,
            CalculationOperationPrimitiveFamily.Scoring => CalculationOperationCapabilityCategory.Scoring,
            CalculationOperationPrimitiveFamily.Ranking => CalculationOperationCapabilityCategory.Ranking,
            CalculationOperationPrimitiveFamily.Transformation => CalculationOperationCapabilityCategory.Transformation,
            _ => CalculationOperationCapabilityCategory.Validation
        };
    }
}
