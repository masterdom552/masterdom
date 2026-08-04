using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class LegacyPostingAdapterTests
{
    [Fact]
    public void Adapt_ShouldMapGeneratedLines_ToLegacyContractParity()
    {
        var source = CreateSourceModel();
        var generator = new PostingLineGenerator(CreateProvider());
        var generated = generator.Generate(source);
        var adapter = new LegacyPostingAdapter();

        var legacy = adapter.Adapt(source, generated, new DateOnly(2026, 8, 31));

        Assert.Equal($"BILL:{source.BillId:N}", legacy.PostingReference);
        Assert.Equal($"JRN-{source.BillNumber}", legacy.JournalNumber);
        Assert.Equal("BILL-202608", legacy.BatchReference);
        Assert.Equal(generated.Lines.Count, legacy.Lines.Count);

        var debitTotal = legacy.Lines.Sum(x => x.DebitAmount);
        var creditTotal = legacy.Lines.Sum(x => x.CreditAmount);
        Assert.Equal(debitTotal, creditTotal);
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
            "BILL-LEG-001",
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
                new BillingSnapshotPostingChargeLineModel("Maintenance", "Maintenance recovery", 200m, "USD")
            ],
            new DateOnly(2026, 8, 1),
            "corr-leg-001");
    }
}
