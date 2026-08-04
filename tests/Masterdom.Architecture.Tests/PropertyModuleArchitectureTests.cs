using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Architecture.Tests;

public sealed class PropertyModuleArchitectureTests
{
    [Fact]
    public void PropertyModule_Namespaces_ShouldNotBeIdentityOwned()
    {
        var propertyAssembly = typeof(Property).Assembly;

        var offendingTypes = propertyAssembly
            .GetTypes()
            .Where(type =>
                type.Namespace is not null &&
                type.Namespace.StartsWith("Masterdom.Modules.Properties", StringComparison.Ordinal) &&
                type.Namespace.Contains(".Identity.", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offendingTypes);
    }

    [Fact]
    public void PropertyModule_ShouldNotReferenceInfrastructureAssembly()
    {
        var referencedAssemblyNames = typeof(Property).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", referencedAssemblyNames);
    }

    [Fact]
    public void Infrastructure_ShouldReferencePropertyModule_ForPersistenceAdaptation()
    {
        var referencedAssemblyNames = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Properties", referencedAssemblyNames);
    }

    [Fact]
    public void PropertyAggregate_ShouldStayIsolatedToPropertyConcepts()
    {
        var propertyType = typeof(Property);

        var methods = propertyType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AssignTenant", methods);
        Assert.DoesNotContain("CreateLease", methods);
        Assert.DoesNotContain("PostBilling", methods);
    }
}
