using Masterdom.Modules.Billing.Contracts.Published.Notifications;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Publication;

public sealed class BillPersistedNotificationTests
{
    [Fact]
    public void Constructor_ShouldAssignRequiredProperties()
    {
        var billId = Guid.NewGuid();
        var executionTimestampUtc = DateTime.UtcNow;
        var propertyId = Guid.NewGuid();

        var notification = new BillPersistedNotification(
            "corr-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            [billId],
            1,
            executionTimestampUtc,
            propertyId);

        Assert.Equal("corr-001", notification.CorrelationId);
        Assert.Equal(new DateOnly(2026, 8, 1), notification.BillingPeriodStartDate);
        Assert.Equal(new DateOnly(2026, 8, 31), notification.BillingPeriodEndDate);
        Assert.Equal(1, notification.PersistedBillCount);
        Assert.Equal(executionTimestampUtc, notification.ExecutionTimestampUtc);
        Assert.Equal(propertyId, notification.PropertyId);
        Assert.Contains(billId, notification.PersistedBillIds);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRequiredValuesAreInvalid()
    {
        var billId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new BillPersistedNotification(
            " ",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            [billId],
            1,
            DateTime.UtcNow));

        Assert.Throws<ArgumentNullException>(() => new BillPersistedNotification(
            "corr-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            null!,
            1,
            DateTime.UtcNow));

        Assert.Throws<ArgumentException>(() => new BillPersistedNotification(
            "corr-001",
            new DateOnly(2026, 8, 31),
            new DateOnly(2026, 8, 1),
            [billId],
            1,
            DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_ShouldEnforceUtcTimestamp()
    {
        var localTimestamp = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => new BillPersistedNotification(
            "corr-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            [Guid.NewGuid()],
            1,
            localTimestamp));
    }

    [Fact]
    public void Constructor_ShouldDefensivelyCopyPersistedBillIds()
    {
        var billIds = new List<Guid> { Guid.NewGuid() };

        var notification = new BillPersistedNotification(
            "corr-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            billIds,
            1,
            DateTime.UtcNow);

        billIds.Add(Guid.NewGuid());

        Assert.Single(notification.PersistedBillIds);
    }
}
