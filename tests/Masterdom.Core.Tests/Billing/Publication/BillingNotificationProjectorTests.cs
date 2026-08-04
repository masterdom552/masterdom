using Masterdom.Core.Identifiers;
using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Modules.Billing.Application.Publication;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Publication;

public sealed class BillingNotificationProjectorTests
{
    [Fact]
    public void ProjectBillPersisted_ShouldCreateNotificationFromCompletedPersistenceState()
    {
        var projector = new BillingNotificationProjector();
        var propertyId = Guid.NewGuid();
        var billOne = CreateBill("BILL-NOTIFY-001", propertyId);
        var billTwo = CreateBill("BILL-NOTIFY-002", propertyId);
        var executionTimestampUtc = DateTime.UtcNow;

        var notification = projector.ProjectBillPersisted(
            "corr-project-001",
            [billOne, billTwo],
            executionTimestampUtc);

        Assert.Equal("corr-project-001", notification.CorrelationId);
        Assert.Equal(new DateOnly(2026, 8, 1), notification.BillingPeriodStartDate);
        Assert.Equal(new DateOnly(2026, 8, 31), notification.BillingPeriodEndDate);
        Assert.Equal(2, notification.PersistedBillCount);
        Assert.Equal(executionTimestampUtc, notification.ExecutionTimestampUtc);
        Assert.Equal(propertyId, notification.PropertyId);
        Assert.Contains(billOne.Id.Value, notification.PersistedBillIds);
        Assert.Contains(billTwo.Id.Value, notification.PersistedBillIds);
    }

    [Fact]
    public void ProjectBillPersisted_ShouldOmitPropertyId_WhenBillsSpanMultipleProperties()
    {
        var projector = new BillingNotificationProjector();

        var notification = projector.ProjectBillPersisted(
            "corr-project-002",
            [CreateBill("BILL-NOTIFY-003", Guid.NewGuid()), CreateBill("BILL-NOTIFY-004", Guid.NewGuid())],
            DateTime.UtcNow);

        Assert.Null(notification.PropertyId);
    }

    [Fact]
    public void ProjectBillPersisted_ShouldThrow_WhenNoBillsAreProvided()
    {
        var projector = new BillingNotificationProjector();

        Assert.Throws<ArgumentException>(() =>
            projector.ProjectBillPersisted("corr-project-003", Array.Empty<BillAggregate>(), DateTime.UtcNow));
    }

    private static BillAggregate CreateBill(string billNumber, Guid propertyId)
    {
        return BillAggregate.Generate(
            BillId.New(),
            BillNumber.Create(billNumber),
            TenancyReference.Create(Guid.NewGuid()),
            LeaseReference.Create(Guid.NewGuid()),
            PropertyReference.Create(propertyId),
            PersonReference.Create(PersonId.New()),
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            GeneratedDate.Create(new DateOnly(2026, 8, 1)),
            IssueDate.Create(new DateOnly(2026, 8, 1)),
            DueDate.Create(new DateOnly(2026, 8, 10)),
            Currency.Create("USD"),
            ChargeCollection.Create([ChargeLine.Create(ChargeKind.Rent, "Rent charge", 1000m)]));
    }
}
