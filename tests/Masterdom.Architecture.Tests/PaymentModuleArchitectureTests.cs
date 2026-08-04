using System.Reflection;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Services;
using Masterdom.Modules.Payment.Contracts.Billing;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Architecture.Tests;

public sealed class PaymentModuleArchitectureTests
{
    [Fact]
    public void PaymentApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(IPaymentApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void PaymentModule_ShouldNotReferenceBillingLedgerOrReportingAssemblies()
    {
        var references = typeof(PaymentAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("Masterdom.Modules.Billing", references);
        Assert.DoesNotContain(references, x => x.Contains("Ledger", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x.Contains("Reporting", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PaymentCommands_ShouldConsumeBillingThroughPublishedContracts()
    {
        var allocationCommandType = typeof(AllocatePaymentCommand);
        var propertyType = allocationCommandType.GetProperty(nameof(AllocatePaymentCommand.BillSettlements))?.PropertyType;

        Assert.Equal(typeof(IReadOnlyCollection<BillSettlementContract>), propertyType);
        Assert.StartsWith(
            "Masterdom.Modules.Payment.Contracts.Billing",
            typeof(BillSettlementContract).Namespace ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void PaymentAggregate_ShouldNotExposeBillingInfrastructureOrReportingTypes()
    {
        var namespaces = typeof(PaymentAggregate)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Billing", StringComparison.Ordinal));
        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Infrastructure", StringComparison.Ordinal));
        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Reporting", StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_ShouldReferencePaymentModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.Payment", references);
    }
}
