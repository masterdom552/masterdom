using System.Reflection;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Architecture.Tests;

public sealed class CalculationEngineMetadataArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void DescriptorProvider_ShouldDiscoverEveryDescriptor()
    {
        var descriptors = LoadDescriptors();

        Assert.Equal(30, descriptors.Count);
        Assert.Equal(descriptors.Count, descriptors.Select(x => x.DescriptorId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(descriptors.Count, descriptors.Select(x => x.CapabilityId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(descriptors.Count, descriptors.Select(x => x.OperationName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Registry_ShouldBeBuiltFromTheDescriptorProvider()
    {
        var registry = new CalculationOperationRegistry();

        Assert.Equal(30, registry.GetAll().Count);
        Assert.All(registry.GetAll(), descriptor => Assert.False(string.IsNullOrWhiteSpace(descriptor.OperationName)));
    }

    [Fact]
    public void Registry_ShouldRemainImmutable()
    {
        var registrySource = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata", "CalculationOperationRegistry.cs"));

        Assert.DoesNotContain("Register(", registrySource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterRange(", registrySource, StringComparison.Ordinal);
        Assert.Contains("ImmutableArray", registrySource, StringComparison.Ordinal);
        Assert.DoesNotContain("DefinedTypes", registrySource, StringComparison.Ordinal);
        Assert.DoesNotContain("Activator.CreateInstance", registrySource, StringComparison.Ordinal);
        Assert.DoesNotContain("ValidateDescriptors", registrySource, StringComparison.Ordinal);
    }

    [Fact]
    public void DescriptorProvider_ShouldDependOnCompositeDiscoveryStrategy()
    {
        var providerSource = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata", "CalculationOperationDescriptorProvider.cs"));

        Assert.Contains("ICompositeCalculationOperationDiscoveryStrategy", providerSource, StringComparison.Ordinal);
        Assert.Contains("CalculationOperationMetadataIntegrityValidator", providerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DefinedTypes", providerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Activator.CreateInstance", providerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Reflection", providerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ReflectionCalculationOperationDiscoveryStrategy", providerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void CompositeDiscoveryStrategy_ShouldOwnStrategyCoordination()
    {
        var strategySource = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata", "CompositeCalculationOperationDiscoveryStrategy.cs"));

        Assert.Contains("ICalculationOperationDiscoveryStrategy", strategySource, StringComparison.Ordinal);
        Assert.Contains("OrderBy", strategySource, StringComparison.Ordinal);
        Assert.Contains("SelectMany", strategySource, StringComparison.Ordinal);
        Assert.Contains("ReflectionCalculationOperationDiscoveryStrategy", strategySource, StringComparison.Ordinal);
        Assert.DoesNotContain("DefinedTypes", strategySource, StringComparison.Ordinal);
    }

    [Fact]
    public void ReflectionDiscoveryStrategy_ShouldOwnReflectionOnly()
    {
        var strategySource = File.ReadAllText(Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata", "ReflectionCalculationOperationDiscoveryStrategy.cs"));

        Assert.Contains("DefinedTypes", strategySource, StringComparison.Ordinal);
        Assert.Contains("Activator.CreateInstance", strategySource, StringComparison.Ordinal);
        Assert.Contains("ICalculationOperationDescriptorSource", strategySource, StringComparison.Ordinal);
        Assert.DoesNotContain("CalculationOperationCatalog", strategySource, StringComparison.Ordinal);
    }

    [Fact]
    public void DescriptorMetadata_ShouldIncludeSourceTypeAndSchemaVersion()
    {
        var descriptors = LoadDescriptors();

        Assert.All(descriptors, descriptor => Assert.Equal("Reflection", descriptor.SourceType));
        Assert.All(descriptors, descriptor => Assert.Equal("1.0", descriptor.SchemaVersion));
    }

    [Fact]
    public void DescriptorMetadata_ShouldIncludeCapabilityCategory()
    {
        var descriptors = LoadDescriptors();

        Assert.Contains(descriptors, descriptor => descriptor.CapabilityCategory == "Aggregation");
        Assert.Contains(descriptors, descriptor => descriptor.CapabilityCategory == "Validation");
        Assert.All(descriptors, descriptor => Assert.False(string.IsNullOrWhiteSpace(descriptor.CapabilityCategory)));
    }

    [Fact]
    public void DescriptorMetadata_ShouldIncludeCompatibilityStatus()
    {
        var descriptors = LoadDescriptors();

        Assert.Contains(descriptors, descriptor => descriptor.CompatibilityStatus == "Supported");
        Assert.Contains(descriptors, descriptor => descriptor.CompatibilityStatus == "Deprecated");
        Assert.Contains(descriptors, descriptor => descriptor.CompatibilityStatus == "Experimental");
        Assert.Contains(descriptors, descriptor => descriptor.CompatibilityStatus == "Obsolete");
        Assert.All(descriptors, descriptor => Assert.False(string.IsNullOrWhiteSpace(descriptor.CompatibilityStatus)));
    }

    [Fact]
    public void RequiredMetadata_ShouldExist()
    {
        var descriptors = LoadDescriptors();

        Assert.All(descriptors, descriptor =>
        {
            Assert.False(string.IsNullOrWhiteSpace(descriptor.DescriptorId));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.CapabilityId));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.OperationVersion));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.OperationName));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Description));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.TimeComplexity));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.SpaceComplexity));
        });
    }

    [Fact]
    public void MetadataLayer_ShouldNotContainExecutionLogic()
    {
        var sourceFiles = Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Metadata"),
                "*.cs",
                SearchOption.AllDirectories)
            .ToArray();

        var forbiddenTokens = new[]
        {
            "Execute(",
            "Run(",
            "Invoke(",
            "Handle(",
            "Process(",
            "BusinessContext",
            "Recommendation",
            "SubsidyOptimization",
            "Configuration",
            "Repository",
            "Service"
        };

        var offenders = sourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void MetadataLayer_ShouldNotDependOnBusinessModulesOrRuntimeAssets()
    {
        var platformProject = Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "Masterdom.Platform.csproj");
        var content = File.ReadAllText(platformProject);

        var forbiddenTokens = new[]
        {
            "Masterdom.Modules.",
            "Masterdom.Infrastructure",
            "BusinessContext",
            "Recommendation",
            "SubsidyOptimization",
            "ImportExport",
            "Configuration"
        };

        Assert.DoesNotContain(forbiddenTokens, token => content.Contains(token, StringComparison.Ordinal));
    }

    private static IReadOnlyList<DescriptorRow> LoadDescriptors()
    {
        var providerType = typeof(CalculationOperationRegistry).Assembly.GetType("Masterdom.Platform.CalculationEngine.Metadata.CalculationOperationDescriptorProvider", throwOnError: true)!;
        var provider = (ICalculationOperationDescriptorProvider)Activator.CreateInstance(providerType, nonPublic: true)!;
        return provider.GetDescriptors()
            .Select(descriptor => new DescriptorRow(
                descriptor.DescriptorId.Value,
                descriptor.SourceType.ToString(),
                descriptor.SchemaVersion,
                descriptor.CapabilityId.Value,
                descriptor.OperationName,
                descriptor.Description,
                descriptor.OperationCategory.ToString(),
                descriptor.CapabilityCategory.ToString(),
                descriptor.CompositionLevel.ToString(),
                descriptor.CompatibilityStatus.ToString(),
                descriptor.OperationVersion.Value,
                descriptor.TimeComplexity,
                descriptor.SpaceComplexity))
            .ToArray();
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

    private sealed record DescriptorRow(
        string DescriptorId,
        string SourceType,
        string SchemaVersion,
        string CapabilityId,
        string OperationName,
        string Description,
        string OperationCategory,
        string CapabilityCategory,
        string CompositionLevel,
        string CompatibilityStatus,
        string OperationVersion,
        string TimeComplexity,
        string SpaceComplexity);
}
