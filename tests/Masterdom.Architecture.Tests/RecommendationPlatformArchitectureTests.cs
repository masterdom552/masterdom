namespace Masterdom.Architecture.Tests;

public sealed class RecommendationPlatformArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void RecommendationPlatform_ShouldNotReferenceBusinessModuleNamespaces()
    {
        var sourceFiles = EnumerateRecommendationSourceFiles();
        var offenders = sourceFiles
            .Where(path => File.ReadAllText(path).Contains("using Masterdom.Modules.", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void RecommendationPlatform_ShouldNotDependOnForbiddenDomainsOrEngines()
    {
        var sourceFiles = EnumerateRecommendationSourceFiles();
        var forbiddenTokens = new[]
        {
            "Masterdom.Modules.SubsidyOptimization",
            "Masterdom.Modules.Billing",
            "Masterdom.Modules.Payment",
            "Masterdom.Modules.Metering",
            "Masterdom.Platform.FormulaEngine",
            "TariffEngine",
            "CalculationEngine",
            "Masterdom.Platform.Notifications",
            "Masterdom.Modules.Reporting",
            "Masterdom.Modules.Documents",
            "Dashboard",
            "AIRecommendationConsumer"
        };

        var offenders = sourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void RecommendationPipeline_ShouldConsumeBusinessContextContracts()
    {
        var pipelinePath = Path.Combine(
            RepositoryRoot,
            "src",
            "Masterdom.Platform",
            "Recommendation",
            "RecommendationPipeline.cs").Replace('\\', '/');

        var content = File.ReadAllText(pipelinePath);

        Assert.Contains("Masterdom.Platform.BusinessContext", content, StringComparison.Ordinal);
    }

    [Fact]
    public void BusinessModules_ShouldNotImplementRecommendationConsumer()
    {
        var moduleFiles = Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(path => path.Replace('\\', '/').Contains("/Masterdom.Modules.", StringComparison.Ordinal))
            .ToList();

        var offenders = moduleFiles
            .Where(path => File.ReadAllText(path).Contains("IRecommendationConsumer", StringComparison.Ordinal))
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    private static IEnumerable<string> EnumerateRecommendationSourceFiles()
    {
        return Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "Recommendation"),
                "*.cs",
                SearchOption.AllDirectories)
            .Select(path => path.Replace('\\', '/'));
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
