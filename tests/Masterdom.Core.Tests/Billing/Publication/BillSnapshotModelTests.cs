using Masterdom.Modules.Billing.Contracts.Published.Models;

namespace Masterdom.Core.Tests.Billing.MonthlyBilling.Publication;

public sealed class BillSnapshotModelTests
{
    [Fact]
    public void Constructor_ShouldAssignBillingSnapshotFacts()
    {
        var line = new BillSnapshotChargeLineModel("Rent", "Base rent", 1000m, "LEASE-001");

        var model = new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-2026-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            "usd",
            1000m,
            1000m,
            [line],
            new DateOnly(2026, 8, 1),
            "corr-001");

        Assert.Equal("BILL-2026-001", model.BillNumber);
        Assert.Equal("USD", model.CurrencyCode);
        Assert.Equal(new DateOnly(2026, 8, 1), model.BillingPeriodStartDate);
        Assert.Equal(new DateOnly(2026, 8, 31), model.BillingPeriodEndDate);
        Assert.Equal(1000m, model.TotalAmount);
        Assert.Equal(1000m, model.OutstandingAmount);
        Assert.Single(model.ChargeLines);
        Assert.Equal("corr-001", model.CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldDefensivelyCopyChargeLines()
    {
        var lines = new List<BillSnapshotChargeLineModel>
        {
            new("Rent", "Base rent", 1000m)
        };

        var model = new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-2026-002",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            "USD",
            1000m,
            1000m,
            lines);

        lines.Add(new BillSnapshotChargeLineModel("Maintenance", "Repair", 25m));

        Assert.Single(model.ChargeLines);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidCurrencyCode()
    {
        var exception = Assert.Throws<ArgumentException>(() => new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-2026-003",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            "US",
            1000m,
            1000m,
            [new BillSnapshotChargeLineModel("Rent", "Base rent", 1000m)]));

        Assert.Equal("Currency code must use ISO-4217 alpha-3 format. (Parameter 'currencyCode')", exception.Message);
    }
}
