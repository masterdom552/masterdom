using Masterdom.Abstractions.Policies;
using Masterdom.Modules.Lease.Application.Services;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Architecture.Tests;

public sealed class PolicyCatalogContractArchitectureTests
{
    [Fact]
    public void Lease_ShouldReferenceSharedAbstractionsWithoutReferencingPolicyFramework()
    {
        var references = typeof(LeasePolicyCatalog).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Abstractions", references);
        Assert.DoesNotContain("Masterdom.Modules.PolicyFramework", references);
    }

    [Fact]
    public void PolicyFramework_ShouldNotReferenceLease()
    {
        var references = typeof(PolicyAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Lease", references);
    }

    [Fact]
    public void Security_ShouldNotReferencePolicyFrameworkImplementation()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var securityModulePath = Path.Combine(repositoryRoot, "src", "Masterdom.Modules.Security");
        var project = File.ReadAllText(Path.Combine(securityModulePath, "Masterdom.Modules.Security.csproj"));
        var sourceFiles = Directory.GetFiles(securityModulePath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        Assert.DoesNotContain("Masterdom.Modules.PolicyFramework", project, StringComparison.Ordinal);
        Assert.All(sourceFiles, path =>
            Assert.DoesNotContain("Masterdom.Modules.PolicyFramework", File.ReadAllText(path), StringComparison.Ordinal));
    }

    [Fact]
    public void Security_ShouldUseInfrastructureOwnedPolicyFrameworkComposition()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Masterdom.Host", "Program.cs"));
        var securityComposition = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Masterdom.Modules.Security",
            "SecurityModuleServiceCollectionExtensions.cs"));

        Assert.Equal(1, CountOccurrences(program, "AddPolicyFrameworkRuntime()"));
        Assert.DoesNotContain("AddPolicyFrameworkRuntime", securityComposition, StringComparison.Ordinal);
        Assert.DoesNotContain("IApplicablePolicyResolver", securityComposition, StringComparison.Ordinal);
    }

    [Fact]
    public void SharedPolicyContract_ShouldNotExposeModuleOrInfrastructureTypes()
    {
        var contractTypes = new[]
        {
            typeof(ApplicablePolicyRequest),
            typeof(ApplicablePolicy),
            typeof(ApplicablePolicyResolution),
            typeof(IApplicablePolicyResolver)
        };

        var exposedTypes = contractTypes
            .SelectMany(type => type.GetProperties().Select(property => property.PropertyType)
                .Concat(type.GetMethods().Select(method => method.ReturnType))
                .Concat(type.GetMethods().SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType))))
            .Select(UnwrapType)
            .Where(type => type.Namespace is not null)
            .ToList();

        Assert.DoesNotContain(exposedTypes, type => type.Namespace!.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(exposedTypes, type => type.Namespace!.StartsWith("Masterdom.Infrastructure", StringComparison.Ordinal));
        Assert.DoesNotContain(exposedTypes, type => type.Namespace!.StartsWith("Masterdom.Modules.Lease", StringComparison.Ordinal));
        Assert.DoesNotContain(exposedTypes, type => type.Namespace!.StartsWith("Masterdom.Modules.PolicyFramework", StringComparison.Ordinal));
    }

    private static Type UnwrapType(Type type)
    {
        if (type.IsGenericType)
        {
            return type.GetGenericArguments().FirstOrDefault(argument => argument.Namespace is not null) ?? type;
        }

        return type;
    }

    private static int CountOccurrences(string content, string value)
    {
        return content.Split(value, StringSplitOptions.None).Length - 1;
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Masterdom.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve repository root from test execution path.");
    }
}
