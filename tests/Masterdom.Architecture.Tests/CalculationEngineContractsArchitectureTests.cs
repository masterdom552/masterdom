using System.Reflection;
using Masterdom.Platform.CalculationEngine.Contracts;

namespace Masterdom.Architecture.Tests;

public sealed class CalculationEngineContractsArchitectureTests
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    [Fact]
    public void Contracts_ShouldNotDependOnForbiddenModulesOrRuntimeServices()
    {
        var sourceFiles = EnumerateContractSourceFiles();
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
            "GetService(",
            "IServiceProvider",
            "ServiceProvider",
            "Resolve<",
            "CalculationOperationRegistry",
            "CalculationOperationDescriptorSource",
            "Repository",
            "DbContext"
        };

        var offenders = sourceFiles
            .Where(path => forbiddenTokens.Any(token => File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(path => path.Replace('\\', '/'))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Contracts_ShouldBeImmutableAndConstructorBacked()
    {
        var contractTypes = typeof(ICalculationEngine).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "Masterdom.Platform.CalculationEngine.Contracts")
            .Where(type => type.IsClass && type.IsPublic)
            .ToArray();

        Assert.NotEmpty(contractTypes);

        foreach (var type in contractTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.False(property.SetMethod?.IsPublic == true, $"{type.Name}.{property.Name} must not expose a public setter.");

                var propertyTypeName = property.PropertyType.FullName ?? property.PropertyType.Name;

                Assert.DoesNotContain("System.Collections.Generic.List", propertyTypeName, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Collections.Generic.Dictionary", propertyTypeName, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Collections.Generic.HashSet", propertyTypeName, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Collections.Generic.ICollection", propertyTypeName, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Collections.Generic.IList", propertyTypeName, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Collections.Generic.IDictionary", propertyTypeName, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void Contracts_ShouldRemainProviderIndependent()
    {
        var contractSource = string.Join(
            Environment.NewLine,
            EnumerateContractSourceFiles().Select(File.ReadAllText));

        Assert.DoesNotContain("Masterdom.Modules.", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ServiceCollection", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("IServiceScope", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Repository", contractSource, StringComparison.Ordinal);
    }

    private static IEnumerable<string> EnumerateContractSourceFiles()
    {
        return Directory.EnumerateFiles(
                Path.Combine(RepositoryRoot, "src", "Masterdom.Platform", "CalculationEngine", "Contracts"),
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
