using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Contracts;

public sealed class ChargeCandidateTests
{
    [Fact]
    public void Constructor_ShouldInitializeAndNormalizeFields()
    {
        var metadata = new Dictionary<string, string>
        {
            ["  SourceId  "] = "  SRC-1  "
        };

        var candidate = new ChargeCandidate(
            " Rent ",
            " Base rent ",
            1000m,
            " usd ",
            " Lease ",
            " ext-1 ",
            metadata);

        Assert.Equal("Rent", candidate.ChargeType);
        Assert.Equal("Base rent", candidate.Description);
        Assert.Equal(1000m, candidate.Amount);
        Assert.Equal("USD", candidate.Currency);
        Assert.Equal("Lease", candidate.SourceCapability);
        Assert.Equal("ext-1", candidate.ExternalReference);
        Assert.Single(candidate.Metadata);
        Assert.Equal("SRC-1", candidate.Metadata["SourceId"]);
    }

    [Fact]
    public void Constructor_ShouldDefaultMetadataToEmptyCollection()
    {
        var candidate = new ChargeCandidate("Rent", "Base rent", 1000m, "USD", "Lease");

        Assert.Empty(candidate.Metadata);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNegative()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ChargeCandidate("Rent", "Base rent", -1m, "USD", "Lease"));

        Assert.Equal("Charge candidate amount cannot be negative.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyIsInvalid()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ChargeCandidate("Rent", "Base rent", 1m, "US", "Lease"));

        Assert.Equal("Currency must use ISO-4217 alpha-3 format.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMetadataContainsEmptyKey()
    {
        var metadata = new Dictionary<string, string>
        {
            [" "] = "value"
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ChargeCandidate("Rent", "Base rent", 1m, "USD", "Lease", metadata: metadata));

        Assert.Equal("Charge candidate metadata key cannot be empty.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMetadataContainsEmptyValue()
    {
        var metadata = new Dictionary<string, string>
        {
            ["key"] = " "
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ChargeCandidate("Rent", "Base rent", 1m, "USD", "Lease", metadata: metadata));

        Assert.Equal("Charge candidate metadata value cannot be empty.", ex.Message);
    }

    [Fact]
    public void Contract_ShouldBeImmutableByPublicApi()
    {
        var mutablePropertyExists = typeof(ChargeCandidate)
            .GetProperties()
            .Any(x => x.SetMethod is not null && x.SetMethod.IsPublic);

        Assert.False(mutablePropertyExists);
    }
}
