namespace Masterdom.Architecture.Tests;

public sealed class BusinessContextPlatformArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void BusinessContextPlatform_ShouldNotReferenceBusinessModuleNamespaces()
    {
        var sourceFiles = EnumerateBusinessContextSourceFiles();
        var offenders = sourceFiles
            .Where(path => File.ReadAllText(path).Contains("using Masterdom.Modules.", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void BusinessContextPlatform_ShouldNotDependOnRecommendationOrSubsidyOrCalculation()
    {
        var sourceFiles = EnumerateBusinessContextSourceFiles();
        var forbiddenTokens = new[]
        {
            "Masterdom.Modules.SubsidyOptimization",
            "Masterdom.Platform.Recommendation",
            "RecommendationPlatform",
            "CalculationEngine"
        };

        var offenders = sourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(offenders);
    }

    private static IEnumerable<string> EnumerateBusinessContextSourceFiles()
    {
        return Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "BusinessContext"),
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
