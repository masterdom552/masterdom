using System.Reflection;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.People.Application.Services;

namespace Masterdom.Architecture.Tests;

public sealed class PersonModuleArchitectureTests
{
    [Fact]
    public void PeopleApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IPersonApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void PeopleApplication_ShouldNotReferencePropertiesModule()
    {
        var references = typeof(IPersonApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Properties", references);
    }

    [Fact]
    public void Infrastructure_ShouldReferencePeopleModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.People", references);
    }

    [Fact]
    public void PersonAggregate_ShouldStayIsolatedToBusinessIdentityConcepts()
    {
        var methods = typeof(Person).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AssignProperty", methods);
        Assert.DoesNotContain("CreateLease", methods);
        Assert.DoesNotContain("Authenticate", methods);
        Assert.DoesNotContain("Authorize", methods);
    }
}
