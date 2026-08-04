namespace Masterdom.Architecture.Tests;

public sealed class GenericCalculationReuseArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void BusinessModules_ShouldNotReference_CalculationEngineImplementationSurfaces()
    {
        var moduleFiles = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains("/Masterdom.Modules.", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(moduleFiles);

        var forbiddenTokens = new[]
        {
            "using Masterdom.Platform.CalculationEngine.Primitives",
            "using Masterdom.Platform.CalculationEngine.Composites",
            "using Masterdom.Platform.CalculationEngine.Execution",
            "using Masterdom.Platform.CalculationEngine.Metadata",
            "CalculationExecutionPipeline",
            "CalculationOperationRegistry",
            "BindingFlags.",
            "Activator.CreateInstance(",
            "GetMethod(",
            "CreateDefaultEngine(",
            "CalculationEngineFactory",
            "new CalculationRuntime(",
            "CalculationPrimitiveExecutionRegistryBuilder",
            "CalculationRuntimeExecutionRegistryBuilder",
            "CalculationCompositeRuntimeOperations"
        };

        var offenders = moduleFiles
            .Where(path =>
            {
                var content = File.ReadAllText(path);
                return forbiddenTokens.Any(token => content.Contains(token, StringComparison.Ordinal));
            })
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void SubsidyOptimizationMigratedCalculationSlices_ShouldUse_CalculationRuntimeCapabilities_InsteadOfLocalMath()
    {
        var migratedFiles = new Dictionary<string, (string[] requiredTokens, string[] forbiddenTokens)>(StringComparer.Ordinal)
        {
            [Path.Combine(RepositoryRoot, "src", "Masterdom.Modules.SubsidyOptimization", "Application", "Maximizer", "ConsumptionEstimator.cs")] =
                (
                    requiredTokens:
                    [
                        "SubsidyCalculationRuntimeInvoker",
                        "aggregation.mean",
                        "aggregation.weighted_mean",
                        "normalization.ratio",
                        "normalization.clamp",
                        "interpolation.weighted_blend"
                    ],
                    forbiddenTokens:
                    [
                        "Average(",
                        "Math.Clamp(",
                        "CalculateWeightedAverage(",
                        "weightedTotal",
                        "weightSum"
                    ]),
            [Path.Combine(RepositoryRoot, "src", "Masterdom.Modules.SubsidyOptimization", "Application", "Maximizer", "ForecastEngine.cs")] =
                (
                    requiredTokens:
                    [
                        "SubsidyCalculationRuntimeInvoker",
                        "forecast.projection"
                    ],
                    forbiddenTokens:
                    [
                        "Average(",
                        " / estimate.WeightedAverageUnits",
                        " * trendFactor",
                        "projected - estimate.OccupancyAdjustedUnits"
                    ]),
            [Path.Combine(RepositoryRoot, "src", "Masterdom.Modules.SubsidyOptimization", "Application", "Maximizer", "ConfidenceScorer.cs")] =
                (
                    requiredTokens:
                    [
                        "SubsidyCalculationRuntimeInvoker",
                        "statistics.spread",
                        "scoring.confidence",
                        "normalization.ratio",
                        "normalization.clamp"
                    ],
                    forbiddenTokens:
                    [
                        "Math.Clamp(",
                        "spread / 100m",
                        "projected.Max() - projected.Min()"
                    ]),
            [Path.Combine(RepositoryRoot, "src", "Masterdom.Modules.SubsidyOptimization", "Application", "Maximizer", "ScenarioEvaluator.cs")] =
                (
                    requiredTokens:
                    [
                        "SubsidyCalculationRuntimeInvoker",
                        "scoring.weighted_score",
                        "ranking.tie_break"
                    ],
                    forbiddenTokens:
                    [
                        ".OrderByDescending(",
                        ".ThenByDescending(",
                        ".ThenBy(",
                        "* 2.5m",
                        "* 1.7m",
                        "* 10m",
                        "* 0.1m"
                    ])
        };

        foreach (var pair in migratedFiles)
        {
            var content = File.ReadAllText(pair.Key);

            Assert.All(pair.Value.requiredTokens, token =>
                Assert.Contains(token, content, StringComparison.Ordinal));

            Assert.All(pair.Value.forbiddenTokens, token =>
                Assert.DoesNotContain(token, content, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void RecommendationAndBusinessContext_ShouldNotAcquire_GenericCalculationOwnership()
    {
        var guardedRoots = new[]
        {
            Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "Recommendation"),
            Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "BusinessContext")
        };

        var sourceFiles = guardedRoots
            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .ToArray();

        Assert.NotEmpty(sourceFiles);

        var forbiddenTokens = new[]
        {
            "using Masterdom.Platform.CalculationEngine",
            "class ConsumptionEstimator",
            "class ForecastEngine",
            "class ConfidenceScorer",
            "class ScenarioEvaluator",
            "class WeightedMean",
            "class WeightedScore"
        };

        var offenders = sourceFiles
            .Where(path =>
            {
                var content = File.ReadAllText(path);
                return forbiddenTokens.Any(token => content.Contains(token, StringComparison.Ordinal));
            })
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void SubsidyRuntimeInvoker_ShouldDependOnlyOn_PublicCalculationRuntimeGateway()
    {
        var invokerPath = Path.Combine(
            RepositoryRoot,
            "src",
            "Masterdom.Modules.SubsidyOptimization",
            "Application",
            "Maximizer",
            "SubsidyCalculationRuntimeInvoker.cs");

        var content = File.ReadAllText(invokerPath);

        Assert.Contains("ICalculationRuntime", content, StringComparison.Ordinal);
        Assert.Contains("CalculationRuntimeRequest", content, StringComparison.Ordinal);
        Assert.Contains("CalculationCapabilityId", content, StringComparison.Ordinal);
        Assert.DoesNotContain("CalculationOperationRegistry", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Masterdom.Platform.CalculationEngine.Metadata", content, StringComparison.Ordinal);
        Assert.DoesNotContain("BindingFlags.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("CalculationEngineFactory", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Masterdom.Platform.CalculationEngine.Execution", content, StringComparison.Ordinal);
    }

    [Fact]
    public void BusinessModules_ShouldOnlyUse_ApprovedCalculationEngineContractSurface()
    {
        var moduleFiles = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains("/Masterdom.Modules.", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(moduleFiles);

        var forbiddenContractTokens = new[]
        {
            "CalculationOperationDescriptorId",
            "CalculationOperationVersion",
            "CalculationOperationCapabilityId",
            "CalculationOperationCapabilityCategory",
            "CalculationOperationCompatibilityStatus",
            "ICalculationRegistry"
        };

        var offenders = moduleFiles
            .Where(path =>
            {
                var content = File.ReadAllText(path);
                return forbiddenContractTokens.Any(token => content.Contains(token, StringComparison.Ordinal));
            })
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void CalculationEngine_ShouldExpose_SingleRegistrationEntrypoint()
    {
        var calculationEngineRoot = Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine");
        var sourceFiles = Directory
            .EnumerateFiles(calculationEngineRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => path.Replace('\\', '/'))
            .ToArray();

        var extensionDefinitions = sourceFiles
            .Where(path => File.ReadAllText(path).Contains("AddCalculationEngine(", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(extensionDefinitions);
        Assert.EndsWith(
            "/src/Masterdom.Platform/CalculationEngine/CalculationEngineServiceCollectionExtensions.cs",
            extensionDefinitions[0],
            StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeImplementation_ShouldRemain_Internal()
    {
        var runtimeType = typeof(Masterdom.Platform.CalculationEngine.Contracts.ICalculationRuntime)
            .Assembly
            .GetType("Masterdom.Platform.CalculationEngine.Execution.CalculationRuntime", throwOnError: true)!;

        Assert.False(runtimeType.IsPublic);
        Assert.False(runtimeType.IsNestedPublic);
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
}
