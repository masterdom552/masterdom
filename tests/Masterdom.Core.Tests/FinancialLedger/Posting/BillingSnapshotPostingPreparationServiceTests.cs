using Masterdom.Modules.Billing.Contracts.Published.Models;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class BillingSnapshotPostingPreparationServiceTests
{
    [Fact]
    public void Prepare_ShouldGeneratePostingRequestAndLegacyContract()
    {
        var service = CreateService();
        var snapshot = CreateSnapshotModel();

        var result = service.Prepare(
            snapshot,
            new DateTimeOffset(2026, 8, 31, 23, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 8, 31));

        Assert.NotNull(result.PostingRequest);
        Assert.NotNull(result.LegacyContract);
        Assert.NotNull(result.PreparedJournal);
        Assert.NotEmpty(result.GeneratedLines.Lines);
        Assert.Equal(result.GeneratedLines.DebitTotal, result.GeneratedLines.CreditTotal);
        Assert.Equal("USD", result.PostingRequest.CurrencyCode);
        Assert.Equal(result.GeneratedLines.Lines.Count, result.PostingRequest.Lines.Length);
        Assert.Equal(result.GeneratedLines.DebitTotal, result.PreparedJournal.DebitTotal);
        Assert.Equal(result.GeneratedLines.CreditTotal, result.PreparedJournal.CreditTotal);
    }

    [Fact]
    public void Prepare_ShouldThrow_WhenValidationFails()
    {
        var service = CreateService();

        var invalidSnapshot = new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-PREP-002",
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
            [new BillSnapshotChargeLineModel("Rent", "Rent charge", 1100m)]);

        var exception = Assert.Throws<InvalidOperationException>(() => service.Prepare(
            invalidSnapshot,
            new DateTimeOffset(2026, 8, 31, 23, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 8, 31)));

        Assert.Contains("Charge totals must equal bill total amount.", exception.Message);
    }

    private static BillingSnapshotPostingPreparationService CreateService()
    {
        return new BillingSnapshotPostingPreparationService(
            new BillingSnapshotTranslator(),
            new BillingSnapshotPostingValidator(),
            new PostingLineGenerator(CreateProvider()),
            new JournalPreparationService(new BusinessJournalNumberGenerator()),
            new BillingFinancialPostingRequestFactory(),
            new LegacyPostingAdapter());
    }

    private static IPostingRuleProvider CreateProvider()
    {
        return new BillingPostingRuleEngine(
            new InMemoryChartOfAccounts(new ChartOfAccountsOptions()),
            new BillingPostingRuleEngineOptions());
    }

    private static BillSnapshotModel CreateSnapshotModel()
    {
        return new BillSnapshotModel(
            Guid.NewGuid(),
            "BILL-PREP-001",
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
            [
                new BillSnapshotChargeLineModel("Rent", "Rent charge", 1000m, "LEASE-001"),
                new BillSnapshotChargeLineModel("Maintenance", "Maintenance", 200m, "WO-001")
            ],
            new DateOnly(2026, 8, 1),
            "corr-prep-001");
    }
}
