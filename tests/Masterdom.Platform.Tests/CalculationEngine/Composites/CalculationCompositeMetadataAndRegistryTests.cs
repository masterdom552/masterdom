using Masterdom.Platform.CalculationEngine.Composites;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.Tests.CalculationEngine.Composites;

public sealed class CalculationCompositeMetadataAndRegistryTests
{
    private static readonly string[] ExpectedCompositeCapabilityIds =
    [
        CompositeCapabilityIds.ConsumptionEstimation,
        CompositeCapabilityIds.ForecastProjection,
        CompositeCapabilityIds.Confidence,
        CompositeCapabilityIds.ScenarioScore,
        CompositeCapabilityIds.ScenarioRanking,
        CompositeCapabilityIds.CanonicalImportConversion,
        CompositeCapabilityIds.Pagination
    ];

    [Fact]
    public void CompositeMetadata_ShouldContainFrozenComposites()
    {
        var registry = new CalculationOperationRegistry();

        var compositeDescriptors = registry
            .ResolveByCompositionLevel(CalculationOperationCompositionLevel.Composite)
            .ToArray();

        Assert.Equal(ExpectedCompositeCapabilityIds.Length, compositeDescriptors.Length);

        foreach (var capabilityId in ExpectedCompositeCapabilityIds)
        {
            Assert.Contains(
                compositeDescriptors,
                descriptor => string.Equals(descriptor.CapabilityId.Value, capabilityId, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void CompositeMetadata_ShouldDeclareOnlyPrimitiveDependencies()
    {
        var operationRegistry = new CalculationOperationRegistry();
        var descriptorsByCapabilityId = operationRegistry
            .GetAll()
            .ToDictionary(descriptor => descriptor.CapabilityId.Value, descriptor => descriptor, StringComparer.OrdinalIgnoreCase);

        var composites = operationRegistry
            .ResolveByCompositionLevel(CalculationOperationCompositionLevel.Composite)
            .ToArray();

        Assert.NotEmpty(composites);

        foreach (var composite in composites)
        {
            Assert.NotEmpty(composite.DependencyCapabilityIds);

            foreach (var dependency in composite.DependencyCapabilityIds)
            {
                var dependencyDescriptor = descriptorsByCapabilityId[dependency.Value];
                Assert.Equal(CalculationOperationCompositionLevel.Primitive, dependencyDescriptor.CompositionLevel);
            }
        }
    }

    [Fact]
    public void CompositeRegistry_ShouldDiscoverAndResolveComposites()
    {
        var registry = new CalculationCompositeRegistry();

        var discovered = registry.DiscoverComposites();

        Assert.Equal(ExpectedCompositeCapabilityIds.Length, discovered.Count);

        var resolvedByCapability = registry.ResolveByCapabilityId(
            CalculationOperationCapabilityId.Create(CompositeCapabilityIds.ScenarioRanking));
        var resolvedByDescriptor = registry.ResolveByDescriptorId(
            CalculationOperationDescriptorId.Create("ce-op-00028"));

        Assert.Equal("Scenario Ranking Composite", resolvedByCapability.OperationName);
        Assert.Equal("ranking.scenario", resolvedByDescriptor.CapabilityId.Value);
    }

    [Fact]
    public void CompositeRegistry_ShouldSupportFamilyCompatibilityAndStabilityLookups()
    {
        var registry = new CalculationCompositeRegistry();

        var scoringComposites = registry.ResolveByFamily(CalculationOperationPrimitiveFamily.Scoring);
        var supportedComposites = registry.ResolveByCompatibility(CalculationOperationCompatibilityStatus.Supported);
        var obsoleteComposites = registry.ResolveByCompatibility(CalculationOperationCompatibilityStatus.Obsolete);
        var stableComposites = registry.ResolveByStability(CalculationOperationStability.Stable);

        Assert.Equal(2, scoringComposites.Count);
        Assert.Equal(6, supportedComposites.Count);
        Assert.Single(obsoleteComposites);
        Assert.Equal(5, stableComposites.Count);
    }

    [Fact]
    public void CompositeContracts_ShouldBeImmutableShapes()
    {
        var assembly = typeof(ConsumptionEstimationCompositeCalculator).Assembly;

        var dtoTypes = assembly.GetTypes()
            .Where(type => type.Namespace == "Masterdom.Platform.CalculationEngine.Composites")
            .Where(type => type.Name.EndsWith("CompositeInput", StringComparison.Ordinal)
                || type.Name.EndsWith("CompositeOutput", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(dtoTypes);
        Assert.All(dtoTypes, type => Assert.True(type.IsSealed));

        foreach (var type in dtoTypes)
        {
            var writableProperties = type
                .GetProperties()
                .Where(property => property.SetMethod is not null)
                .Select(property => property.Name)
                .ToArray();

            Assert.Empty(writableProperties);
        }
    }
}
