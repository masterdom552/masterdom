using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class JournalPreparationServiceTests
{
    [Fact]
    public void Prepare_ShouldCreateBalancedInMemoryJournal()
    {
        var source = CreateSourceModel();
        var generator = new PostingLineGenerator(CreateProvider());
        var generated = generator.Generate(source);
        var service = new JournalPreparationService(new BusinessJournalNumberGenerator());

        var journal = service.Prepare(source, generated, new DateOnly(2026, 8, 31));

        Assert.Equal(journal.DebitTotal, journal.CreditTotal);
        Assert.Equal("USD", journal.CurrencyCode);
        Assert.Equal(source.BillId, journal.BillId);
        Assert.Equal(source.BillNumber, journal.BillNumber);
        Assert.NotEmpty(journal.Lines);
        Assert.Equal(JournalLifecycleState.Prepared, journal.LifecycleState);
        Assert.Equal($"BILL:{source.BillId:N}", journal.PostingReference);
        Assert.StartsWith("JRN-BILLING-", journal.JournalNumber, StringComparison.Ordinal);
        Assert.DoesNotContain("/", journal.JournalNumber, StringComparison.Ordinal);
    }

    [Fact]
    public void PreparedJournal_ShouldRejectUnbalancedLines()
    {
        var lines = new List<PreparedJournalLine>
        {
            new("line-1", "1100", "Accounts Receivable", FinancialPostingDirection.Debit, 100m, "USD", "Debit"),
            new("line-2", "4100", "Rental Revenue", FinancialPostingDirection.Credit, 90m, "USD", "Credit")
        };

        Assert.Throws<InvalidOperationException>(() => new PreparedJournal(
            "JRN-TEST-001",
            "BILL:TEST",
            "JRN-BILLING-20260831-ABC123",
            new DateOnly(2026, 8, 31),
            "USD",
            "Test",
            "BATCH-1",
            "billing",
            Guid.NewGuid(),
            "BILL-TEST-001",
            lines));
    }

    private static IPostingRuleProvider CreateProvider()
    {
        return new BillingPostingRuleEngine(
            new InMemoryChartOfAccounts(new ChartOfAccountsOptions()),
            new BillingPostingRuleEngineOptions());
    }

    private static BillingSnapshotPostingSourceModel CreateSourceModel()
    {
        return new BillingSnapshotPostingSourceModel(
            Guid.NewGuid(),
            "BILL-JRN-001",
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
                new BillingSnapshotPostingChargeLineModel("Rent", "Rent charge", 1000m, "USD"),
                new BillingSnapshotPostingChargeLineModel("LateFee", "Late fee", 200m, "USD")
            ],
            new DateOnly(2026, 8, 1),
            "corr-jrn-001");
    }
}
