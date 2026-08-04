using Masterdom.Modules.FinancialLedger.Application.Posting;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class DeterministicJournalNumberGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnUniqueNumbers_ForSameInputs()
    {
        var generator = new DeterministicJournalNumberGenerator();

        var first = generator.Generate("billing", new DateOnly(2026, 8, 31), "BILL:abcdef");
        var second = generator.Generate("billing", new DateOnly(2026, 8, 31), "BILL:abcdef");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Generate_ShouldNotRequirePostingReferenceToShapeBusinessIdentity()
    {
        var generator = new DeterministicJournalNumberGenerator();

        var first = generator.Generate("billing", new DateOnly(2026, 8, 31), "BILL:abcdef");
        var second = generator.Generate("billing", new DateOnly(2026, 8, 31), "BILL:ghijkl");

        Assert.StartsWith("JRN-BILLING-20260831-", first, StringComparison.Ordinal);
        Assert.StartsWith("JRN-BILLING-20260831-", second, StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_ShouldHonorConfiguredFormat()
    {
        var generator = new DeterministicJournalNumberGenerator(new JournalNumberingOptions
        {
            Prefix = "GL",
            Format = "{prefix}/{source}/{date}/{sequence}",
            SequenceLength = 8
        });

        var number = generator.Generate("billing", new DateOnly(2026, 8, 31), "BILL:abcdef");

        Assert.StartsWith("GL/BILLING/20260831/", number, StringComparison.Ordinal);
        Assert.Equal("GL/BILLING/20260831/".Length + 8, number.Length);
    }
}
