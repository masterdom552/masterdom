using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class PostingLineGeneratorTests
{
    [Fact]
    public void Generate_ShouldCreateDebitAndCreditLines()
    {
        var generator = new PostingLineGenerator(CreateProvider());

        var result = generator.Generate(CreateSourceModel());

        Assert.Equal(3, result.Lines.Count);
        Assert.Contains(result.Lines, x => x.Direction == FinancialPostingDirection.Debit);
        Assert.Equal(2, result.Lines.Count(x => x.Direction == FinancialPostingDirection.Credit));
    }

    [Fact]
    public void Generate_ShouldBalanceDebitsAndCredits()
    {
        var generator = new PostingLineGenerator(CreateProvider());

        var result = generator.Generate(CreateSourceModel());

        Assert.Equal(result.DebitTotal, result.CreditTotal);
        Assert.Equal(1200m, result.DebitTotal);
    }

    [Fact]
    public void Generate_ShouldApplySnapshotCurrencyToAllLines()
    {
        var generator = new PostingLineGenerator(CreateProvider());

        var result = generator.Generate(CreateSourceModel());

        Assert.All(result.Lines, x => Assert.Equal("USD", x.CurrencyCode));
    }

    [Fact]
    public void Generate_ShouldApplyPolicyAccounts_ForKnownAndUnknownCategories()
    {
        var generator = new PostingLineGenerator(CreateProvider());

        var source = new BillingSnapshotPostingSourceModel(
            Guid.NewGuid(),
            "BILL-GEN-002",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            "USD",
            310m,
            310m,
            [
                new BillingSnapshotPostingChargeLineModel("UtilityReference", "Utility usage", 100m, "USD"),
                new BillingSnapshotPostingChargeLineModel("UnexpectedCategory", "Unknown category", 210m, "USD")
            ]);

        var result = generator.Generate(source);

        Assert.Contains(result.Lines, x => x.Direction == FinancialPostingDirection.Credit && x.AccountCode == "4200");
        Assert.Contains(result.Lines, x => x.Direction == FinancialPostingDirection.Credit && x.AccountCode == "4999");
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
            "BILL-GEN-001",
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
                new BillingSnapshotPostingChargeLineModel("Rent", "Rent charge", 1000m, "USD", "LEASE-001"),
                new BillingSnapshotPostingChargeLineModel("Maintenance", "Maintenance recovery", 200m, "USD", "WO-001")
            ],
            new DateOnly(2026, 8, 1),
            "corr-gen-001");
    }
}
