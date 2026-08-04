using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Lease.Application.Services;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Architecture.Tests;

public sealed class LeaseModuleArchitectureTests
{
    [Fact]
    public void LeaseApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(ILeaseApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void LeaseModule_ShouldNotReferencePropertyPeopleOrTenancyModules()
    {
        var references = typeof(LeaseAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Properties", references);
        Assert.DoesNotContain("Masterdom.Modules.People", references);
        Assert.DoesNotContain("Masterdom.Modules.Tenancy", references);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceLeaseModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Lease", references);
    }

    [Fact]
    public void LeaseAggregate_ShouldExposeReferenceTypesOnly_ForCrossContextLinks()
    {
        var leaseType = typeof(LeaseAggregate);

        Assert.Equal("TenancyReference", leaseType.GetProperty("Tenancy")?.PropertyType.Name);
        Assert.Equal("PropertyReference", leaseType.GetProperty("Property")?.PropertyType.Name);
        Assert.Equal("UnitReference", leaseType.GetProperty("Unit")?.PropertyType.Name);
        Assert.Equal("PersonReference", leaseType.GetProperty("Person")?.PropertyType.Name);

        var propertyTypes = leaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Properties", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.People", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Tenancy", StringComparison.Ordinal));
    }
}
