using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Tenancy.Application.Services;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Architecture.Tests;

public sealed class TenancyModuleArchitectureTests
{
    [Fact]
    public void TenancyApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(ITenancyApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void TenancyModule_ShouldNotReferencePeopleOrPropertiesModules()
    {
        var references = typeof(TenancyAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.People", references);
        Assert.DoesNotContain("Masterdom.Modules.Properties", references);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceTenancyModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Tenancy", references);
    }

    [Fact]
    public void TenancyAggregate_ShouldNotExposePeopleOrPropertyAggregateMutators()
    {
        var methods = typeof(TenancyAggregate).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AssignProperty", methods);
        Assert.DoesNotContain("MergePerson", methods);
        Assert.DoesNotContain("CreateProperty", methods);
    }
}
