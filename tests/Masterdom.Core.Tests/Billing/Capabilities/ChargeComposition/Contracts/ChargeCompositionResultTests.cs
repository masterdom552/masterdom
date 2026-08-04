using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Contracts;

public sealed class ChargeCompositionResultTests
{
    [Fact]
    public void Constructor_ShouldInitializeChargeCandidatesOnly()
    {
        var chargeCandidates = new[]
        {
            new ChargeCandidate("Rent", "Base rent", 1000m, "USD", "Lease")
        };

        var result = new ChargeCompositionResult(chargeCandidates);

        Assert.Single(result.ChargeCandidates);
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("Charges"));
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("ExecutedProviders"));
        Assert.Null(typeof(ChargeCompositionResult).GetProperty("Warnings"));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenChargesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChargeCompositionResult(null!));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenChargeCandidatesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChargeCompositionResult(null!));
    }
}
