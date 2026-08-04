using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Metering.Application.Services;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Architecture.Tests;

public sealed class MeteringModuleArchitectureTests
{
    [Fact]
    public void MeteringApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IMeteringApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void MeteringModule_ShouldNotReferenceRestrictedBusinessModules()
    {
        var references = typeof(MeterAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Billing", references);
        Assert.DoesNotContain("Masterdom.Modules.Properties", references);
        Assert.DoesNotContain("Masterdom.Modules.People", references);
        Assert.DoesNotContain("Masterdom.Modules.Tenancy", references);
        Assert.DoesNotContain("Masterdom.Modules.Lease", references);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceMeteringModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Metering", references);
    }

    [Fact]
    public void MeteringAggregate_ShouldExposeIdentifierReferencesOnly_ForCrossContextLinks()
    {
        var meterType = typeof(MeterAggregate);

        Assert.Equal("MeterLocationReference", meterType.GetProperty("MeterLocationReference")?.PropertyType.Name);

        var propertyTypes = meterType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Billing", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Properties", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.People", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Tenancy", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Lease", StringComparison.Ordinal));
    }
}
