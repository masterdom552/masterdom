using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Billing;
using Masterdom.Modules.Billing.Application.Services;
using Masterdom.Modules.Billing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Architecture.Tests;

public sealed class BillingModuleArchitectureTests
{
    [Fact]
    public void BillingApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IBillingApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void BillingModule_ShouldNotReferenceRestrictedModules()
    {
        var references = typeof(BillAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Properties", references);
        Assert.DoesNotContain("Masterdom.Modules.People", references);
        Assert.DoesNotContain("Masterdom.Modules.Tenancy", references);
        Assert.DoesNotContain("Masterdom.Modules.Lease", references);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceBillingModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Billing", references);
    }

    [Fact]
    public void BillingAggregate_ShouldExposeReferenceTypesOnly_ForCrossContextLinks()
    {
        var billType = typeof(BillAggregate);

        Assert.Equal("TenancyReference", billType.GetProperty("TenancyReference")?.PropertyType.Name);
        Assert.Equal("LeaseReference", billType.GetProperty("LeaseReference")?.PropertyType.Name);
        Assert.Equal("PropertyReference", billType.GetProperty("PropertyReference")?.PropertyType.Name);
        Assert.Equal("PersonReference", billType.GetProperty("BilledParty")?.PropertyType.Name);

        var propertyTypes = billType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Properties", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.People", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Tenancy", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyTypes, x => x.StartsWith("Masterdom.Modules.Lease", StringComparison.Ordinal));
    }

    [Fact]
    public void BillingApplication_ShouldNotUseDbContextDirectly()
    {
        var assembly = typeof(IBillingApplicationService).Assembly;

        var applicationTypes = assembly.GetTypes()
            .Where(type => type.Namespace is not null)
            .Where(type => type.Namespace.StartsWith("Masterdom.Modules.Billing.Application", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(applicationTypes);

        foreach (var type in applicationTypes)
        {
            var constructorParameterTypes = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .SelectMany(ctor => ctor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .ToList();

            Assert.DoesNotContain(typeof(MasterdomDbContext), constructorParameterTypes);
        }
    }

    [Fact]
    public void BillingAggregate_Constructors_ShouldRemainEfBindable()
    {
        var constructors = typeof(BillAggregate)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotEmpty(constructors);

        foreach (var constructor in constructors)
        {
            var parameterTypes = constructor
                .GetParameters()
                .Select(x => x.ParameterType)
                .ToList();

            Assert.DoesNotContain(parameterTypes, type =>
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
        }

        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseNpgsql("Host=localhost;Database=masterdom_billing_architecture;Username=postgres;Password=postgres")
            .Options;

        using var dbContext = new MasterdomDbContext(options);

        var exception = Record.Exception(() => _ = dbContext.Model);
        Assert.Null(exception);

        var entityType = dbContext.Model.FindEntityType(typeof(BillAggregate));
        Assert.NotNull(entityType);

        var versionsNavigation = entityType!.FindNavigation(nameof(BillAggregate.Versions));
        Assert.NotNull(versionsNavigation);
    }

    [Fact]
    public void BillingRepository_ShouldImplementAggregateRootRepositoryContract()
    {
        var repositoryType = typeof(BillRepository);

        Assert.Contains(typeof(IBillRepository), repositoryType.GetInterfaces());

        var getById = typeof(IBillRepository).GetMethod("GetById");
        Assert.NotNull(getById);
        Assert.Equal(typeof(BillAggregate), getById!.ReturnType);

        var getByNumber = typeof(IBillRepository).GetMethod("GetByNumber");
        Assert.NotNull(getByNumber);
        Assert.Equal(typeof(BillAggregate), getByNumber!.ReturnType);
    }
}
