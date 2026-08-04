using System;
using System.Collections.Immutable;
using System.Linq;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.Tests.CalculationEngine.Metadata;

public sealed class CalculationOperationRegistryTests
{
    [Fact]
    public void DescriptorProvider_ShouldDiscoverEveryDescriptor()
    {
        var provider = new CalculationOperationDescriptorProvider();
        var descriptors = provider.GetDescriptors();

        Assert.Equal(30, descriptors.Count);
        Assert.Contains(descriptors, item => item.DescriptorId.Value == "ce-op-00001");
        Assert.Contains(descriptors, item => item.OperationCategory == CalculationOperationCategory.Composite);
        Assert.All(descriptors, item => Assert.Equal(CalculationOperationDescriptorSourceType.Reflection, item.SourceType));
        Assert.All(descriptors, item => Assert.Equal("1.0", item.SchemaVersion));
    }

    [Fact]
    public void CompositeDiscoveryStrategy_ShouldMergeDescriptorSets()
    {
        var strategy = new CompositeCalculationOperationDiscoveryStrategy(
        [
            new ZuluDiscoveryStrategy(),
            new AlphaDiscoveryStrategy()
        ]);

        var descriptors = strategy.GetDescriptors();

        Assert.Equal(2, descriptors.Count);
        Assert.Equal(["alpha-1", "zulu-1"], descriptors.Select(descriptor => descriptor.DescriptorId.Value).ToArray());
    }

    [Fact]
    public void DescriptorProvider_ShouldValidateDescriptorsFromTheDiscoveryStrategy()
    {
        var provider = new CalculationOperationDescriptorProvider(new DuplicateDescriptorDiscoveryStrategy());

        Assert.Throws<CalculationOperationValidationException>(() => provider.GetDescriptors());
    }

    [Fact]
    public void CompositeDiscoveryStrategy_ShouldPreserveDeterministicOrdering()
    {
        var strategy = new CompositeCalculationOperationDiscoveryStrategy(
        [
            new ZuluDiscoveryStrategy(),
            new AlphaDiscoveryStrategy()
        ]);

        var descriptors = strategy.GetDescriptors();

        Assert.Equal(["alpha-1", "zulu-1"], descriptors.Select(descriptor => descriptor.DescriptorId.Value).ToArray());
    }

    [Fact]
    public void DescriptorProvider_ShouldUseTheSuppliedDiscoveryStrategy()
    {
        var strategy = new CompositeCalculationOperationDiscoveryStrategy([
            new StubDiscoveryStrategy(new[]
            {
                BuildDescriptor("ce-op-stub-1", "validation.stub_one", "stub operation 1"),
                BuildDescriptor("ce-op-stub-2", "validation.stub_two", "stub operation 2")
            })]);

        var provider = new CalculationOperationDescriptorProvider(strategy);

        var descriptors = provider.GetDescriptors();

        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, descriptor => descriptor.DescriptorId.Value == "ce-op-stub-1");
        Assert.Contains(descriptors, descriptor => descriptor.OperationName == "stub operation 2");
    }

    [Fact]
    public void ReflectionDiscoveryStrategy_ShouldReturnImmutableDescriptors()
    {
        var strategy = new ReflectionCalculationOperationDiscoveryStrategy();

        var descriptors = strategy.GetDescriptors();

        Assert.Equal(30, descriptors.Count);
        Assert.IsType<ImmutableArray<ICalculationOperationDescriptor>>(descriptors);
    }

    [Fact]
    public void Registry_ShouldBeBuiltFromTheDescriptorProvider()
    {
        var registry = new CalculationOperationRegistry();

        Assert.NotEmpty(registry.GetAll());
        Assert.Equal(30, registry.GetAll().Count);
        Assert.Contains(registry.GetAll(), item => item.DescriptorId.Value == "ce-op-00001");
    }

    [Fact]
    public void ResolveByDescriptorId_ShouldReturnRegisteredOperation()
    {
        var registry = new CalculationOperationRegistry();

        var descriptor = registry.ResolveByDescriptorId(CalculationOperationDescriptorId.Create("ce-op-00015"));

        Assert.Equal("Scoring Confidence", descriptor.OperationName);
        Assert.Equal(CalculationOperationCapabilityId.Create("scoring.confidence"), descriptor.CapabilityId);
    }

    [Fact]
    public void ResolveByCapabilityId_ShouldReturnRegisteredOperation()
    {
        var registry = new CalculationOperationRegistry();

        var descriptor = registry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("scoring.confidence"));

        Assert.Equal("Scoring Confidence", descriptor.OperationName);
        Assert.Equal(CalculationOperationPrimitiveFamily.Scoring, descriptor.PrimitiveFamily);
    }

    [Fact]
    public void ResolveByOperationName_ShouldBeCaseInsensitive()
    {
        var registry = new CalculationOperationRegistry();

        var descriptor = registry.ResolveByOperationName("aggregation sum");

        Assert.Equal("aggregation.sum", descriptor.CapabilityId.Value);
    }

    [Fact]
    public void ResolveByPrimitiveFamily_ShouldReturnMatchingDescriptors()
    {
        var registry = new CalculationOperationRegistry();

        var descriptors = registry.ResolveByPrimitiveFamily(CalculationOperationPrimitiveFamily.Ranking);

        Assert.Equal(4, descriptors.Count);
        Assert.All(descriptors, descriptor => Assert.Equal(CalculationOperationPrimitiveFamily.Ranking, descriptor.PrimitiveFamily));
    }

    [Fact]
    public void GetByCapabilityCategory_ShouldReturnMatchingDescriptors()
    {
        var registry = new CalculationOperationRegistry();

        var descriptors = registry.GetByCapabilityCategory(CalculationOperationCapabilityCategory.Ranking);

        Assert.Equal(4, descriptors.Count);
        Assert.All(descriptors, descriptor => Assert.Equal(CalculationOperationCapabilityCategory.Ranking, descriptor.CapabilityCategory));
    }

    [Fact]
    public void ResolveByCompositionLevel_ShouldReturnMatchingDescriptors()
    {
        var registry = new CalculationOperationRegistry();

        var descriptors = registry.ResolveByCompositionLevel(CalculationOperationCompositionLevel.Composite);

        Assert.Equal(7, descriptors.Count);
        Assert.All(descriptors, descriptor => Assert.Equal(CalculationOperationCompositionLevel.Composite, descriptor.CompositionLevel));
    }

    [Fact]
    public void ResolveByCompatibilityStatus_ShouldReturnMatchingDescriptors()
    {
        var registry = new CalculationOperationRegistry();

        var supportedDescriptors = registry.ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus.Supported);
        var deprecatedDescriptors = registry.ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus.Deprecated);
        var experimentalDescriptors = registry.ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus.Experimental);
        var obsoleteDescriptors = registry.ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus.Obsolete);

        Assert.Equal(26, supportedDescriptors.Count);
        Assert.Single(deprecatedDescriptors);
        Assert.Equal(2, experimentalDescriptors.Count);
        Assert.Single(obsoleteDescriptors);

        Assert.All(supportedDescriptors, descriptor => Assert.Equal(CalculationOperationCompatibilityStatus.Supported, descriptor.CompatibilityStatus));
        Assert.All(deprecatedDescriptors, descriptor => Assert.Equal(CalculationOperationCompatibilityStatus.Deprecated, descriptor.CompatibilityStatus));
        Assert.All(experimentalDescriptors, descriptor => Assert.Equal(CalculationOperationCompatibilityStatus.Experimental, descriptor.CompatibilityStatus));
        Assert.All(obsoleteDescriptors, descriptor => Assert.Equal(CalculationOperationCompatibilityStatus.Obsolete, descriptor.CompatibilityStatus));
    }

    [Fact]
    public void Registry_ShouldBeImmutableAfterConstruction()
    {
        var mutators = typeof(CalculationOperationRegistry)
            .GetMethods()
            .Where(method => method.IsPublic && method.DeclaringType == typeof(CalculationOperationRegistry))
            .Select(method => method.Name)
            .ToArray();

        Assert.DoesNotContain("Register", mutators);
        Assert.DoesNotContain("RegisterRange", mutators);
        Assert.DoesNotContain("CalculationOperationCatalog", typeof(CalculationOperationRegistry).Assembly.GetTypes().Select(type => type.Name));
    }

    [Fact]
    public void Registry_ShouldIgnoreSourceMetadataAndSchemaVersion()
    {
        var registry = new CalculationOperationRegistry(
        [
            BuildDescriptor("ce-op-reg-1", "capability.registry-1", "registry operation 1") with
            {
                SourceType = CalculationOperationDescriptorSourceType.Generated,
                SchemaVersion = "2.0"
            },
            BuildDescriptor("ce-op-reg-2", "capability.registry-2", "registry operation 2") with
            {
                SourceType = CalculationOperationDescriptorSourceType.Plugin,
                SchemaVersion = "9.9"
            }
        ]);

        Assert.Equal(CalculationOperationDescriptorSourceType.Generated, registry.GetAll().First().SourceType);
        Assert.Equal("2.0", registry.ResolveByDescriptorId(CalculationOperationDescriptorId.Create("ce-op-reg-1")).SchemaVersion);
        Assert.Equal("registry operation 2", registry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("capability.registry-2")).OperationName);
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectOrphanDescriptors()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptors = new[]
        {
            BuildDescriptor(
                "ce-op-orphan-1",
                "validation.orphan",
                "orphan composite",
                dependencyCapabilityIds: ["validation.missing"],
                compositionLevel: CalculationOperationCompositionLevel.Composite,
                operationCategory: CalculationOperationCategory.Composite,
                executionClassification: CalculationOperationExecutionClassification.Composite)
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate(descriptors));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectCyclicDependencies()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var first = BuildDescriptor(
            "ce-op-cycle-1",
            "validation.cycle_one",
            "cycle one",
            dependencyCapabilityIds: ["validation.cycle_two"]);

        var second = BuildDescriptor(
            "ce-op-cycle-2",
            "validation.cycle_two",
            "cycle two",
            dependencyCapabilityIds: ["validation.cycle_one"]);

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([first, second]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectInvalidCapabilityIds()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-invalid-capability",
            "Invalid Capability",
            "invalid capability");

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectInvalidCapabilityCategories()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-invalid-category",
            "validation.invalid_category",
            "invalid category") with
        {
            CapabilityCategory = (CalculationOperationCapabilityCategory)99
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectCapabilityCategoryMismatches()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-category-mismatch",
            "validation.category_mismatch",
            "category mismatch",
            capabilityCategory: CalculationOperationCapabilityCategory.Validation,
            primitiveFamily: CalculationOperationPrimitiveFamily.Aggregation);

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectUnsupportedSchemaVersions()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-schema-1",
            "validation.schema",
            "schema version",
            schemaVersion: "2.0");

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    [Fact]
    public void DescriptorProvider_ShouldRejectDuplicateCompositeRegistrations()
    {
        var provider = new CalculationOperationDescriptorProvider(
            new CompositeCalculationOperationDiscoveryStrategy(
            [
                new DuplicateCompositeDiscoveryStrategy("first"),
                new DuplicateCompositeDiscoveryStrategy("second")
            ]));

        Assert.Throws<CalculationOperationValidationException>(() => provider.GetDescriptors());
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectInvalidStabilityRelationships()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var dependency = BuildDescriptor(
            "ce-op-stability-dependency",
            "validation.stability_dependency",
            "stability dependency",
            stability: CalculationOperationStability.Experimental);

        var descriptor = BuildDescriptor(
            "ce-op-stability-dependent",
            "validation.stability_dependent",
            "stability dependent",
            dependencyCapabilityIds: ["validation.stability_dependency"],
            stability: CalculationOperationStability.Fundamental);

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([dependency, descriptor]));
    }

    [Fact]
    public void IntegrityValidator_ShouldAcceptOutOfOrderDependencies()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var dependency = BuildDescriptor(
            "ce-op-graph-1",
            "validation.graph_dependency",
            "graph dependency");

        var dependent = BuildDescriptor(
            "ce-op-graph-2",
            "validation.graph_dependent",
            "graph dependent",
            dependencyCapabilityIds: ["validation.graph_dependency"]);

        var warnings = validator.Validate([dependent, dependency]);

        Assert.Empty(warnings);
    }

    [Fact]
    public void IntegrityValidator_ShouldWarnOnDeprecatedDescriptors()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-deprecated-1",
            "validation.deprecated",
            "deprecated descriptor",
            compatibilityStatus: CalculationOperationCompatibilityStatus.Deprecated);

        var warnings = validator.Validate([descriptor]);

        Assert.Single(warnings);
        Assert.Contains("deprecated", warnings[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectObsoleteDescriptorsReferencedByComposites()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var obsoleteDependency = BuildDescriptor(
            "ce-op-obsolete-1",
            "validation.obsolete_dependency",
            "obsolete dependency",
            compatibilityStatus: CalculationOperationCompatibilityStatus.Obsolete);

        var composite = BuildDescriptor(
            "ce-op-obsolete-2",
            "validation.obsolete_composite",
            "obsolete composite",
            dependencyCapabilityIds: ["validation.obsolete_dependency"],
            compositionLevel: CalculationOperationCompositionLevel.Composite,
            operationCategory: CalculationOperationCategory.Composite,
            executionClassification: CalculationOperationExecutionClassification.Composite);

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([obsoleteDependency, composite]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRejectInvalidCompatibilityStatuses()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-compatibility-1",
            "validation.invalid_compatibility",
            "invalid compatibility") with
        {
            CompatibilityStatus = (CalculationOperationCompatibilityStatus)99
        };

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    [Fact]
    public void IntegrityValidator_ShouldRequireExplicitSchemaVersionForExperimentalDescriptors()
    {
        var validator = new CalculationOperationMetadataIntegrityValidator();

        var descriptor = BuildDescriptor(
            "ce-op-experimental-1",
            "validation.experimental",
            "experimental descriptor",
            schemaVersion: string.Empty,
            compatibilityStatus: CalculationOperationCompatibilityStatus.Experimental);

        Assert.Throws<CalculationOperationValidationException>(() => validator.Validate([descriptor]));
    }

    private static CalculationOperationDescriptor BuildDescriptor(
        string descriptorId,
        string capabilityId,
        string operationName,
        IReadOnlyCollection<string>? dependencyCapabilityIds = null,
        string schemaVersion = "1.0",
        CalculationOperationCapabilityCategory capabilityCategory = CalculationOperationCapabilityCategory.Aggregation,
        CalculationOperationStability stability = CalculationOperationStability.Fundamental,
        CalculationOperationCompatibilityStatus compatibilityStatus = CalculationOperationCompatibilityStatus.Supported,
        CalculationOperationPrimitiveFamily primitiveFamily = CalculationOperationPrimitiveFamily.Aggregation,
        CalculationOperationCompositionLevel compositionLevel = CalculationOperationCompositionLevel.Primitive,
        CalculationOperationCategory operationCategory = CalculationOperationCategory.Primitive,
        CalculationOperationExecutionClassification executionClassification = CalculationOperationExecutionClassification.Primitive)
    {
        return new CalculationOperationDescriptor
        {
            DescriptorId = CalculationOperationDescriptorId.Create(descriptorId),
            SourceType = CalculationOperationDescriptorSourceType.Test,
            SchemaVersion = schemaVersion,
            OperationName = operationName,
            CapabilityId = CalculationOperationCapabilityId.Create(capabilityId),
            OperationVersion = CalculationOperationVersion.Create("1.0.0"),
            Description = "Test descriptor.",
            PrimitiveFamily = primitiveFamily,
            CapabilityCategory = capabilityCategory,
            CompositionLevel = compositionLevel,
            OperationCategory = operationCategory,
            ExecutionClassification = executionClassification,
            Purity = CalculationOperationPurity.Pure,
            Determinism = CalculationOperationDeterminism.Deterministic,
            Stability = stability,
            CompatibilityStatus = compatibilityStatus,
            TimeComplexity = "O(1)",
            SpaceComplexity = "O(1)",
            DependencyCapabilityIds = (dependencyCapabilityIds ?? Array.Empty<string>()).Select(CalculationOperationCapabilityId.Create).ToArray(),
            TechnicalTags = ["primitive"],
            MathematicalTags = ["mean"]
        };
    }

    private sealed class DuplicateCompositeDiscoveryStrategy : ICalculationOperationDiscoveryStrategy
    {
        private readonly string _suffix;

        public DuplicateCompositeDiscoveryStrategy(string suffix)
        {
            _suffix = suffix;
        }

        public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
        {
            return [
                BuildDescriptor(
                    "ce-op-duplicate-composite",
                    "validation.duplicate_composite",
                    $"duplicate composite {_suffix}",
                    dependencyCapabilityIds: ["aggregation.mean"],
                    compositionLevel: CalculationOperationCompositionLevel.Composite,
                    operationCategory: CalculationOperationCategory.Composite,
                    executionClassification: CalculationOperationExecutionClassification.Composite)
            ];
        }
    }

    private sealed class StubDiscoveryStrategy : ICompositeCalculationOperationDiscoveryStrategy
    {
        private readonly IReadOnlyCollection<ICalculationOperationDescriptor> _descriptors;

        public StubDiscoveryStrategy(IReadOnlyCollection<ICalculationOperationDescriptor> descriptors)
        {
            _descriptors = descriptors;
        }

        public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
        {
            return _descriptors;
        }
    }

    private sealed class AlphaDiscoveryStrategy : ICalculationOperationDiscoveryStrategy
    {
        public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
        {
            return new[]
            {
                BuildDescriptor("alpha-1", "capability.alpha", "alpha operation")
            };
        }
    }

    private sealed class ZuluDiscoveryStrategy : ICalculationOperationDiscoveryStrategy
    {
        public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
        {
            return new[]
            {
                BuildDescriptor("zulu-1", "capability.zulu", "zulu operation")
            };
        }
    }

    private sealed class DuplicateDescriptorDiscoveryStrategy : ICompositeCalculationOperationDiscoveryStrategy
    {
        public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
        {
            return new[]
            {
                BuildDescriptor("ce-op-dup-1", "capability.one", "operation one"),
                BuildDescriptor("ce-op-dup-1", "capability.two", "operation two")
            };
        }
    }
}
