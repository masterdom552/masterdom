namespace Masterdom.Architecture.Tests;

public sealed class CalculationEngineCompositeArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void CompositeLayer_ShouldNotReferenceBusinessWorkflowsRepositoriesOrConfiguration()
    {
        var compositeSources = Directory
            .EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Composites"),
                "*.cs",
                SearchOption.AllDirectories)
            .ToArray();

        Assert.NotEmpty(compositeSources);

        var forbiddenTokens = new[]
        {
            "Masterdom.Modules.",
            "Subsidy",
            "Billing",
            "Workflow",
            "Repository",
            "DbContext",
            "Configuration",
            "IConfiguration"
        };

        var offenders = compositeSources
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void PrimitiveLayer_ShouldNotDependOnCompositeLayer()
    {
        var primitiveSources = Directory
            .EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Primitives"),
                "*.cs",
                SearchOption.AllDirectories)
            .ToArray();

        Assert.NotEmpty(primitiveSources);

        var offenders = primitiveSources
            .Where(path => File.ReadAllText(path).Contains("Masterdom.Platform.CalculationEngine.Composites", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void CompositeMetadata_ShouldDependOnLevel1PrimitiveCapabilitiesOnly()
    {
        var metadataSource = File.ReadAllText(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Masterdom.Platform",
                "CalculationEngine",
                "Metadata",
                "CalculationOperationDescriptorSources.cs"));

        Assert.Contains("estimation.consumption", metadataSource, StringComparison.Ordinal);
        Assert.Contains("forecast.projection", metadataSource, StringComparison.Ordinal);
        Assert.Contains("scoring.confidence_composite", metadataSource, StringComparison.Ordinal);
        Assert.Contains("scoring.scenario", metadataSource, StringComparison.Ordinal);
        Assert.Contains("ranking.scenario", metadataSource, StringComparison.Ordinal);
        Assert.Contains("transformation.import_canonical", metadataSource, StringComparison.Ordinal);
        Assert.Contains("validation.pagination", metadataSource, StringComparison.Ordinal);

        Assert.DoesNotContain("Masterdom.Modules.", metadataSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Subsidy", metadataSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Billing", metadataSource, StringComparison.Ordinal);
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
