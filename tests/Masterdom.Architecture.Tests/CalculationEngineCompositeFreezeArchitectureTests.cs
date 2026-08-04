using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Architecture.Tests;

public sealed class CalculationEngineCompositeFreezeArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    private static readonly FrozenCompositeDescriptor[] FrozenDescriptors =
    [
        new("ce-op-00024", "estimation.consumption", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Stable,
            ["aggregation.mean", "aggregation.weighted_mean", "normalization.ratio", "interpolation.weighted_blend", "normalization.clamp"]),
        new("ce-op-00025", "forecast.projection", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Stable,
            ["normalization.ratio", "projection.trend_factor", "projection.threshold_variance"]),
        new("ce-op-00026", "scoring.confidence_composite", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Stable,
            ["statistics.spread", "normalization.clamp", "scoring.confidence"]),
        new("ce-op-00027", "scoring.scenario", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Stable,
            ["scoring.weighted_score", "normalization.clamp"]),
        new("ce-op-00028", "ranking.scenario", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Stable,
            ["ranking.order", "ranking.tie_break", "ranking.top_n"]),
        new("ce-op-00029", "transformation.import_canonical", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Supported, CalculationOperationStability.Experimental,
            ["transformation.canonical_date", "transformation.canonical_number", "transformation.canonical_boolean", "validation.range"]),
        new("ce-op-00030", "validation.pagination", "1.0", "1.0.0", CalculationOperationCompatibilityStatus.Obsolete, CalculationOperationStability.Fundamental,
            ["normalization.bounds_guard", "normalization.ratio"])
    ];

    [Fact]
    public void FrozenCompositeDescriptors_ShouldPreserveIdentityAndSemantics()
    {
        var registry = new CalculationOperationRegistry();
        var composites = registry.ResolveByCompositionLevel(CalculationOperationCompositionLevel.Composite)
            .ToDictionary(descriptor => descriptor.CapabilityId.Value, descriptor => descriptor, StringComparer.OrdinalIgnoreCase);

        foreach (var expected in FrozenDescriptors)
        {
            Assert.True(composites.TryGetValue(expected.CapabilityId, out var actual), $"Frozen capability '{expected.CapabilityId}' was not found.");

            Assert.Equal(expected.DescriptorId, actual.DescriptorId.Value);
            Assert.Equal(CalculationOperationCompositionLevel.Composite, actual.CompositionLevel);
            Assert.Equal(expected.ContractVersion, actual.OperationVersion.Value);
            Assert.Equal(expected.DescriptorVersion, actual.SchemaVersion);
            Assert.Equal(expected.CompatibilityStatus, actual.CompatibilityStatus);
            Assert.Equal(expected.Stability, actual.Stability);
            Assert.Equal(expected.Dependencies, actual.DependencyCapabilityIds.Select(dependency => dependency.Value).ToArray());
        }
    }

    [Fact]
    public void CompositeRegistry_ShouldRemainMetadataOnly()
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata", "CalculationCompositeRegistry.cs"));

        var forbiddenTokens = new[]
        {
            "Execute(",
            "ICalculationEngine",
            "CalculationExecutionPipeline",
            "CalculationEngineFactory",
            "IConfigurationResolver",
            "Repository",
            "Workflow",
            "Masterdom.Modules.Subsidy",
            "Masterdom.Modules.Billing"
        };

        Assert.DoesNotContain(forbiddenTokens, token => source.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void CompositeExecutionBoundary_ShouldRemainPipelineOwned()
    {
        var compositeDirectory = Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Composites");
        var compositeSources = Directory.EnumerateFiles(compositeDirectory, "*.cs", SearchOption.AllDirectories).ToArray();

        var forbiddenTokens = new[]
        {
            "new CalculationExecutionMetadata(",
            "new CalculationResult(",
            "new CalculationExecutionPipeline("
        };

        var offenders = compositeSources
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(offenders);

        var supportSource = File.ReadAllText(Path.Combine(compositeDirectory, "CalculationCompositeSupport.cs"));
        Assert.Contains("CalculationEngineFactory.CreateDefault()", supportSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Level2CompositeLayer_ShouldReferenceOnlyApprovedCalculationEngineSurfaces()
    {
        var compositeDirectory = Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Composites");
        var compositeSources = Directory.EnumerateFiles(compositeDirectory, "*.cs", SearchOption.AllDirectories).ToArray();

        var allowedPrefixes = new[]
        {
            "using System",
            "using Masterdom.Platform.CalculationEngine.Contracts",
            "using Masterdom.Platform.CalculationEngine.Execution",
            "using Masterdom.Platform.CalculationEngine.Primitives",
            "using Masterdom.Platform.CalculationEngine.Metadata"
        };

        foreach (var sourcePath in compositeSources)
        {
            var usingDirectives = File.ReadAllLines(sourcePath)
                .Select(line => line.Trim())
                .Where(line => line.StartsWith("using ", StringComparison.Ordinal))
                .ToArray();

            foreach (var usingDirective in usingDirectives)
            {
                Assert.Contains(allowedPrefixes, prefix => usingDirective.StartsWith(prefix, StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public void Level1Layers_ShouldHaveNoReferenceToLevel2CompositeNamespace()
    {
        var calculationEngineDirectory = Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine");

        var level1Sources = Directory
            .EnumerateFiles(calculationEngineDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Replace('\\', '/').Contains("/Composites/", StringComparison.Ordinal))
            .ToArray();

        var offenders = level1Sources
            .Where(path => File.ReadAllText(path).Contains("Masterdom.Platform.CalculationEngine.Composites", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Masterdom.slnx")))
            {
                return current.FullName.Replace('\\', '/');
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test execution path.");
    }

    private sealed record FrozenCompositeDescriptor(
        string DescriptorId,
        string CapabilityId,
        string DescriptorVersion,
        string ContractVersion,
        CalculationOperationCompatibilityStatus CompatibilityStatus,
        CalculationOperationStability Stability,
        string[] Dependencies);
}
