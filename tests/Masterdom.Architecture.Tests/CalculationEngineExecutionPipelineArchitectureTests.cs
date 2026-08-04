using System.Reflection;

namespace Masterdom.Architecture.Tests;

public sealed class CalculationEngineExecutionPipelineArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void ExecutionPipeline_ShouldNotDependOnBusinessModules()
    {
        var sourceFiles = EnumerateExecutionPipelineSourceFiles();
        var forbiddenTokens = new[]
        {
            "Masterdom.Modules.",
            "Masterdom.Infrastructure",
            "Masterdom.Platform.BusinessContext",
            "Masterdom.Platform.Recommendation",
            "SubsidyOptimization",
            "Reporting",
            "LanguageSupport",
            "Notifications",
            "Documents",
            "ConfigurationResolver",
            "IServiceProvider",
            "GetService("
        };

        var offenders = sourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void ExecutionPipeline_ShouldExposeOnlyInternalComponents()
    {
        var executionTypes = typeof(Masterdom.Platform.CalculationEngine.Execution.CalculationExecutionPipeline).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "Masterdom.Platform.CalculationEngine.Execution")
            .ToArray();

        Assert.NotEmpty(executionTypes);
        Assert.All(executionTypes, type => Assert.False(type.IsPublic));
    }

    [Fact]
    public void ExecutionPipeline_ShouldContainSingleExecutionPath()
    {
        var pipelineSource = File.ReadAllText(Path.Combine(
            RepositoryRoot,
            "src",
            "Masterdom.Platform",
            "CalculationEngine",
            "Execution",
            "CalculationExecutionPipeline.cs"));

        Assert.Contains("_requestValidator.Validate(request);", pipelineSource, StringComparison.Ordinal);
        Assert.Contains("_operationResolver.Resolve(request);", pipelineSource, StringComparison.Ordinal);
        Assert.Contains("_executor.Execute(definition, request);", pipelineSource, StringComparison.Ordinal);
        Assert.Contains("_resultValidator.Validate(result, definition, executionMetadata);", pipelineSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ExecutionPipeline_MetadataTypes_ShouldRemainInternalAndImmutable()
    {
        var descriptorType = typeof(Masterdom.Platform.CalculationEngine.Execution.CalculationExecutionPipeline).Assembly
            .GetType("Masterdom.Platform.CalculationEngine.Execution.CalculationExecutionPipelineDescriptor", throwOnError: true)!;

        var recordType = typeof(Masterdom.Platform.CalculationEngine.Execution.CalculationExecutionPipeline).Assembly
            .GetType("Masterdom.Platform.CalculationEngine.Execution.CalculationExecutionRecord", throwOnError: true)!;

        Assert.False(descriptorType.IsPublic);
        Assert.False(recordType.IsPublic);

        Assert.All(descriptorType.GetProperties(BindingFlags.Public | BindingFlags.Instance), property =>
        {
            Assert.False(property.SetMethod?.IsPublic == true, $"{descriptorType.Name}.{property.Name} must not expose a public setter.");
        });

        Assert.All(recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance), property =>
        {
            Assert.False(property.SetMethod?.IsPublic == true, $"{recordType.Name}.{property.Name} must not expose a public setter.");
        });
    }

    private static IEnumerable<string> EnumerateExecutionPipelineSourceFiles()
    {
        return Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Execution"),
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
