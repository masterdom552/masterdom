using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Services;
using Masterdom.Modules.UtilityRating.Contracts.Metering;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Architecture.Tests;

public sealed class UtilityRatingModuleArchitectureTests
{
    [Fact]
    public void UtilityRatingApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IUtilityRatingApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void UtilityRatingModule_ShouldNotReferenceBillingPaymentOrLedgerModules()
    {
        var references = typeof(UtilityRatingAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("Masterdom.Modules.Billing", references);
        Assert.DoesNotContain(references, x => x.Contains("Payment", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Contains("Ledger", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UtilityRatingModule_ShouldNotReferenceMeteringAssembly_Directly()
    {
        var references = typeof(UtilityRatingAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Modules.Metering", references);
    }

    [Fact]
    public void UtilityRating_ShouldConsumeMeteringThroughPublishedContractBoundary()
    {
        var commandType = typeof(RateConsumptionCommand);
        var consumptionProperty = commandType.GetProperty(nameof(RateConsumptionCommand.ConsumptionOutput));

        Assert.NotNull(consumptionProperty);
        Assert.Equal(typeof(MeteringConsumptionOutputContract), consumptionProperty!.PropertyType);
        Assert.StartsWith(
            "Masterdom.Modules.UtilityRating.Contracts",
            consumptionProperty.PropertyType.Namespace ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceUtilityRatingModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.UtilityRating", references);
    }

    [Fact]
    public void UtilityRatingAggregate_ShouldExposeReferenceTypesOnly_ForCrossContextLinks()
    {
        var aggregateType = typeof(UtilityRatingAggregate);

        Assert.Equal("MeterReference", aggregateType.GetProperty("MeterReference")?.PropertyType.Name);
        Assert.Equal("ConsumptionReference", aggregateType.GetProperty("ConsumptionReference")?.PropertyType.Name);

        var propertyNamespaces = aggregateType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(propertyNamespaces, x => x.StartsWith("Masterdom.Modules.Billing", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNamespaces, x => x.StartsWith("Masterdom.Modules.Metering", StringComparison.Ordinal));
    }
}
