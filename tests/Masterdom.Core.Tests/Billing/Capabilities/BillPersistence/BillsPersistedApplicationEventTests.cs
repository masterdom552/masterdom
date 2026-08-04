using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Events;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Capabilities.BillPersistence;

public sealed class BillsPersistedApplicationEventTests
{
    [Fact]
    public void Constructor_ShouldImplementBillingApplicationEvent()
    {
        var billId = BillId.New();
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));

        var applicationEvent = new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: [billId],
            persistedBillCount: 1,
            executionTimestampUtc: DateTime.UtcNow);

        Assert.IsAssignableFrom<IBillingApplicationEvent>(applicationEvent);
    }

    [Fact]
    public void Constructor_ShouldAssignRequiredProperties()
    {
        var billId = BillId.New();
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));
        var timestampUtc = DateTime.UtcNow;
        var propertyReference = PropertyReference.Create(Guid.NewGuid());

        var applicationEvent = new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: [billId],
            persistedBillCount: 1,
            executionTimestampUtc: timestampUtc,
            propertyReference: propertyReference);

        Assert.Equal("corr-001", applicationEvent.CorrelationId);
        Assert.Equal(billingPeriod, applicationEvent.BillingPeriod);
        Assert.Equal(1, applicationEvent.PersistedBillCount);
        Assert.Equal(timestampUtc, applicationEvent.ExecutionTimestampUtc);
        Assert.Equal(propertyReference, applicationEvent.PropertyReference);
        Assert.Contains(billId, applicationEvent.PersistedBillIds);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRequiredValuesAreInvalid()
    {
        var billId = BillId.New();
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));

        Assert.Throws<ArgumentException>(() => new BillsPersistedApplicationEvent(
            correlationId: " ",
            billingPeriod: billingPeriod,
            persistedBillIds: [billId],
            persistedBillCount: 1,
            executionTimestampUtc: DateTime.UtcNow));

        Assert.Throws<ArgumentNullException>(() => new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: null!,
            persistedBillIds: [billId],
            persistedBillCount: 1,
            executionTimestampUtc: DateTime.UtcNow));

        Assert.Throws<ArgumentNullException>(() => new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: null!,
            persistedBillCount: 1,
            executionTimestampUtc: DateTime.UtcNow));

        Assert.Throws<ArgumentException>(() => new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: Array.Empty<BillId>(),
            persistedBillCount: 0,
            executionTimestampUtc: DateTime.UtcNow));

        Assert.Throws<ArgumentException>(() => new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: [billId],
            persistedBillCount: 2,
            executionTimestampUtc: DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_ShouldEnforceUtcTimestamp()
    {
        var billId = BillId.New();
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));
        var localTimestamp = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: [billId],
            persistedBillCount: 1,
            executionTimestampUtc: localTimestamp));
    }

    [Fact]
    public void Constructor_ShouldDefensivelyCopyPersistedBillIds()
    {
        var billIds = new List<BillId> { BillId.New() };
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));

        var applicationEvent = new BillsPersistedApplicationEvent(
            correlationId: "corr-001",
            billingPeriod: billingPeriod,
            persistedBillIds: billIds,
            persistedBillCount: 1,
            executionTimestampUtc: DateTime.UtcNow);

        billIds.Add(BillId.New());

        Assert.Single(applicationEvent.PersistedBillIds);
    }
}
