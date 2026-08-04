using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class BillingSnapshotPostingValidatorTests
{
    [Fact]
    public void Validate_ShouldSucceed_ForValidInput()
    {
        var validator = new BillingSnapshotPostingValidator();

        var result = validator.Validate(CreateSourceModel());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldReject_MixedLineCurrency()
    {
        var validator = new BillingSnapshotPostingValidator();
        var source = CreateSourceModel(
            chargeLines:
            [
                new BillingSnapshotPostingChargeLineModel("Rent", "Rent charge", 1000m, "USD"),
                new BillingSnapshotPostingChargeLineModel("Maintenance", "Maintenance", 200m, "EUR")
            ]);

        var result = validator.Validate(source);

        Assert.False(result.IsValid);
        Assert.Contains("Charge line currency must match bill snapshot currency.", result.Errors);
    }

    [Fact]
    public void Validate_ShouldReject_MissingIdentifiersAndReferences()
    {
        var validator = new BillingSnapshotPostingValidator();
        var source = new BillingSnapshotPostingSourceModel(
            Guid.Empty,
            string.Empty,
            new DateOnly(2026, 8, 31),
            new DateOnly(2026, 8, 1),
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 1),
            "US",
            0m,
            -1m,
            []);

        var result = validator.Validate(source);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldReject_IncompleteChargeCategory()
    {
        var validator = new BillingSnapshotPostingValidator();
        var source = CreateSourceModel(
            chargeLines:
            [
                new BillingSnapshotPostingChargeLineModel(" ", "Rent charge", 1200m, "USD")
            ]);

        var result = validator.Validate(source);

        Assert.False(result.IsValid);
        Assert.Contains("Charge category is required for each charge line.", result.Errors);
    }

    private static BillingSnapshotPostingSourceModel CreateSourceModel(
        IReadOnlyCollection<BillingSnapshotPostingChargeLineModel>? chargeLines = null)
    {
        var lines = chargeLines ??
            [new BillingSnapshotPostingChargeLineModel("Rent", "Rent charge", 1200m, "USD")];

        return new BillingSnapshotPostingSourceModel(
            Guid.NewGuid(),
            "BILL-VAL-001",
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
            lines,
            new DateOnly(2026, 8, 1),
            "corr-val-001");
    }
}
