using Masterdom.Modules.Billing.Contracts.Published.Models;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Translation;

public sealed class BillingSnapshotTranslatorTests
{
    [Fact]
    public void Translate_ShouldMapDeterministically()
    {
        var translator = new BillingSnapshotTranslator();
        var snapshot = CreateSnapshotModel();

        var first = translator.Translate(snapshot);
        var second = translator.Translate(snapshot);

        Assert.Equal(first.BillId, second.BillId);
        Assert.Equal(first.BillNumber, second.BillNumber);
        Assert.Equal(first.CurrencyCode, second.CurrencyCode);
        Assert.Equal(first.ChargeLines.Count, second.ChargeLines.Count);
    }

    [Fact]
    public void Translate_ShouldMapCompleteBillingFacts()
    {
        var translator = new BillingSnapshotTranslator();
        var snapshot = CreateSnapshotModel();

        var result = translator.Translate(snapshot);

        Assert.Equal(snapshot.BillId, result.BillId);
        Assert.Equal(snapshot.BillNumber, result.BillNumber);
        Assert.Equal(snapshot.BillingPeriodStartDate, result.BillingPeriodStartDate);
        Assert.Equal(snapshot.BillingPeriodEndDate, result.BillingPeriodEndDate);
        Assert.Equal(snapshot.PropertyId, result.PropertyId);
        Assert.Equal(snapshot.TenancyId, result.TenancyId);
        Assert.Equal(snapshot.LeaseId, result.LeaseId);
        Assert.Equal(snapshot.IssueDate, result.IssueDate);
        Assert.Equal(snapshot.DueDate, result.DueDate);
        Assert.Equal(snapshot.CurrencyCode, result.CurrencyCode);
        Assert.Equal(snapshot.TotalAmount, result.TotalAmount);
        Assert.Equal(snapshot.OutstandingAmount, result.OutstandingAmount);

        var line = Assert.Single(result.ChargeLines);
        Assert.Equal(snapshot.CurrencyCode, line.CurrencyCode);
        Assert.Equal("Rent", line.ChargeCategory);
        Assert.Equal(1200m, line.Amount);
    }

    [Fact]
    public void Translate_ShouldThrow_WhenSnapshotIsNull()
    {
        var translator = new BillingSnapshotTranslator();

        Assert.Throws<ArgumentNullException>(() => translator.Translate(null!));
    }

    private static BillSnapshotModel CreateSnapshotModel()
    {
        return new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-TR-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            "USD",
            1200m,
            1200m,
            [new BillSnapshotChargeLineModel("Rent", "Rent charge", 1200m, "LEASE-TR-001")],
            new DateOnly(2026, 8, 1),
            "corr-tr-001");
    }
}
