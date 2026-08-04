using System.Reflection;
using Masterdom.Modules.Billing.Contracts.Published.Models;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Billing.Contracts.Published.Notifications;
using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Services;
using Masterdom.Modules.FinancialLedger.Application.Translation;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Contracts.Payment;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Architecture.Tests;

public sealed class FinancialLedgerModuleArchitectureTests
{
    [Fact]
    public void FinancialLedgerApplication_ShouldNotReferenceInfrastructureAssembly()
    {
        var references = typeof(ILedgerApplicationService).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Masterdom.Infrastructure", references);
    }

    [Fact]
    public void FinancialLedgerModule_ShouldNotReferencePaymentOrReportingAssemblies()
    {
        var references = typeof(LedgerAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain("Masterdom.Modules.Payment", references);
        Assert.DoesNotContain(references, x => x.Contains("Reporting", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FinancialLedgerCommands_ShouldConsumeBillingAndPaymentThroughPublishedContracts()
    {
        Assert.Equal(
            typeof(BillingLedgerPostingContract),
            typeof(PostBillingJournalCommand).GetProperty(nameof(PostBillingJournalCommand.Contract))?.PropertyType);

        Assert.Equal(
            typeof(PaymentLedgerPostingContract),
            typeof(PostPaymentJournalCommand).GetProperty(nameof(PostPaymentJournalCommand.Contract))?.PropertyType);
    }

    [Fact]
    public void FinancialLedgerTranslation_ShouldConsumeBillingThroughPublishedNotificationOnly()
    {
        var translateMethod = typeof(BillingNotificationTranslator).GetMethod(nameof(BillingNotificationTranslator.TranslateBillPersisted));

        Assert.NotNull(translateMethod);
        Assert.Equal(typeof(BillPersistedNotification), translateMethod!.GetParameters().Single().ParameterType);
        Assert.StartsWith(
            "Masterdom.Modules.Billing.Contracts.Published.Notifications",
            typeof(BillPersistedNotification).Namespace ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void FinancialLedgerTranslation_ShouldConsumeBillingSnapshotThroughPublishedModelOnly()
    {
        var translateMethod = typeof(BillingSnapshotTranslator).GetMethod(nameof(BillingSnapshotTranslator.Translate));

        Assert.NotNull(translateMethod);
        Assert.Equal(typeof(BillSnapshotModel), translateMethod!.GetParameters().Single().ParameterType);
        Assert.StartsWith(
            "Masterdom.Modules.Billing.Contracts.Published.Models",
            typeof(BillSnapshotModel).Namespace ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void FinancialLedgerAggregate_ShouldNotExposeBillingOrPaymentNamespaces()
    {
        var namespaces = typeof(LedgerAggregate)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.PropertyType.Namespace ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Billing", StringComparison.Ordinal));
        Assert.DoesNotContain(namespaces, x => x.StartsWith("Masterdom.Modules.Payment", StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_ShouldReferenceFinancialLedgerModule_ForPersistenceAdaptation()
    {
        var references = typeof(MasterdomDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Masterdom.Modules.FinancialLedger", references);
    }
}
