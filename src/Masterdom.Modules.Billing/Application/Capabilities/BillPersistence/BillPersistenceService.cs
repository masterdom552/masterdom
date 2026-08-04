using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Events;
using Masterdom.Modules.Billing.Application.Publication;
using Masterdom.Modules.Billing.Application.Support;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;

public sealed class BillPersistenceService : IBillPersistenceService
{
    private const string MonthlyBillingPersistOperation = "MonthlyBillingPersistBills";

    private readonly BillPersistenceOperation _operation;
    private readonly IBillingPlatformOrchestrator _platformOrchestrator;
    private readonly BillingNotificationProjector _notificationProjector;

    public BillPersistenceService(
        BillPersistenceOperation operation,
        IBillingPlatformOrchestrator platformOrchestrator,
        BillingNotificationProjector notificationProjector)
    {
        _operation = operation ?? throw new ArgumentNullException(nameof(operation));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
        _notificationProjector = notificationProjector ?? throw new ArgumentNullException(nameof(notificationProjector));
    }

    public BillPersistenceResult Persist(BillPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var persistedBills = _operation.Execute(request);

        if (persistedBills.Count == 0)
        {
            return new BillPersistenceResult(Array.Empty<BillAggregate>());
        }

        var executionTimestampUtc = DateTime.UtcNow;
        var bills = persistedBills.ToList();
        var firstBill = bills[0];
        var persistedBillIds = bills.Select(x => x.Id).ToList();
        var distinctPropertyReferences = bills
            .Select(x => x.PropertyReference)
            .Distinct()
            .ToList();

        var applicationEvent = new BillsPersistedApplicationEvent(
            MonthlyBillingPersistOperation,
            firstBill.CurrentSnapshot.BillingPeriod,
            persistedBillIds,
            persistedBillIds.Count,
            executionTimestampUtc,
            distinctPropertyReferences.Count == 1 ? distinctPropertyReferences[0] : null);

        _platformOrchestrator.Publish(applicationEvent);

        _ = _notificationProjector.ProjectBillPersisted(
            MonthlyBillingPersistOperation,
            bills,
            executionTimestampUtc);

        return new BillPersistenceResult(bills);
    }
}
