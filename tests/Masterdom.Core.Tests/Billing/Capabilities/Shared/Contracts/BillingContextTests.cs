using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.Capabilities.Shared.Contracts;

public sealed class BillingContextTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllFields()
    {
        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));
        var billingCycle = BillingCycle.Monthly;
        var propertyReference = PropertyReference.Create(Guid.NewGuid());
        var unitReference = Guid.NewGuid();
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc);
        var executionTimestampUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc);

        var context = new BillingContext(
            billingPeriod,
            billingCycle,
            asOfUtc,
            executionTimestampUtc,
            propertyReference,
            unitReference,
            "  corr-123  ");

        Assert.Equal(billingPeriod, context.BillingPeriod);
        Assert.Equal(billingCycle, context.BillingCycle);
        Assert.Equal(propertyReference, context.PropertyReference);
        Assert.Equal(unitReference, context.UnitReference);
        Assert.Equal(asOfUtc, context.AsOfUtc);
        Assert.Equal(executionTimestampUtc, context.ExecutionTimestampUtc);
        Assert.Equal("corr-123", context.CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldAllowOptionalScopesToBeNull()
    {
        var context = new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc),
            propertyReference: null,
            unitReference: null,
            correlationId: null);

        Assert.Null(context.PropertyReference);
        Assert.Null(context.UnitReference);
        Assert.Null(context.CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBillingPeriodIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BillingContext(
            null!,
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBillingCycleIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            null!,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAsOfUtcIsNotUtc()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Local),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc)));

        Assert.Equal("AsOfUtc must be UTC.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExecutionTimestampUtcIsNotUtc()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            new DateTime(2026, 8, 1, 0, 5, 0, DateTimeKind.Local)));

        Assert.Equal("ExecutionTimestampUtc must be UTC.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitReferenceIsEmptyGuid()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc),
            unitReference: Guid.Empty));

        Assert.Equal("UnitReference cannot be an empty GUID when provided.", ex.Message);
    }

    [Fact]
    public void Contract_ShouldBeImmutableByPublicApi()
    {
        var mutablePropertyExists = typeof(BillingContext)
            .GetProperties()
            .Any(x => x.SetMethod is not null && x.SetMethod.IsPublic);

        Assert.False(mutablePropertyExists);
    }

    [Fact]
    public void Record_ShouldSupportValueEquality()
    {
        var period = BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));
        var cycle = BillingCycle.Monthly;
        var propertyReference = PropertyReference.Create(Guid.NewGuid());
        var unitReference = Guid.NewGuid();
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc);
        var executionTimestampUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc);

        var left = new BillingContext(period, cycle, asOfUtc, executionTimestampUtc, propertyReference, unitReference, "corr-1");
        var right = new BillingContext(period, cycle, asOfUtc, executionTimestampUtc, propertyReference, unitReference, "corr-1");

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
