using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Core.Tests.Payment;

/// <summary>
/// Proves each repaired payment value object's ToString() returns its Value,
/// satisfying the ValueObjectValueConverter contract and preventing
/// varchar(50) overflow on payment persistence columns
/// (PKG-PAYMENT-VALUEOBJECT-TOSTRING-PERSISTENCE-REPAIR).
/// </summary>
public sealed class PaymentValueObjectToStringTests
{
    // ── PaymentMethod ───────────────────────────────────────────────────────

    [Fact]
    public void PaymentMethod_Cash_ToString_ReturnsValue()
        => Assert.Equal("Cash", PaymentMethod.Cash.ToString());

    [Fact]
    public void PaymentMethod_BankTransfer_ToString_ReturnsValue()
        => Assert.Equal("BankTransfer", PaymentMethod.BankTransfer.ToString());

    [Fact]
    public void PaymentMethod_Check_ToString_ReturnsValue()
        => Assert.Equal("Check", PaymentMethod.Check.ToString());

    [Fact]
    public void PaymentMethod_Card_ToString_ReturnsValue()
        => Assert.Equal("Card", PaymentMethod.Card.ToString());

    [Fact]
    public void PaymentMethod_Manual_ToString_ReturnsValue()
        => Assert.Equal("Manual", PaymentMethod.Manual.ToString());

    [Theory]
    [InlineData("Cash")]
    [InlineData("BankTransfer")]
    [InlineData("Check")]
    [InlineData("Card")]
    [InlineData("Manual")]
    public void PaymentMethod_ToString_Length_WithinVarcharFiftyLimit(string value)
    {
        var method = PaymentMethod.Create(value);
        Assert.True(method.ToString().Length <= 50,
            $"PaymentMethod.ToString() produced '{method}' ({method.ToString().Length} chars) — exceeds varchar(50) column limit.");
    }

    // ── PaymentStatus ───────────────────────────────────────────────────────

    [Fact]
    public void PaymentStatus_Received_ToString_ReturnsValue()
        => Assert.Equal("Received", PaymentStatus.Received.ToString());

    [Fact]
    public void PaymentStatus_PartiallyAllocated_ToString_ReturnsValue()
        => Assert.Equal("PartiallyAllocated", PaymentStatus.PartiallyAllocated.ToString());

    [Fact]
    public void PaymentStatus_Allocated_ToString_ReturnsValue()
        => Assert.Equal("Allocated", PaymentStatus.Allocated.ToString());

    [Fact]
    public void PaymentStatus_Reversed_ToString_ReturnsValue()
        => Assert.Equal("Reversed", PaymentStatus.Reversed.ToString());

    [Fact]
    public void PaymentStatus_Voided_ToString_ReturnsValue()
        => Assert.Equal("Voided", PaymentStatus.Voided.ToString());

    [Theory]
    [InlineData("Received")]
    [InlineData("PartiallyAllocated")]
    [InlineData("Allocated")]
    [InlineData("Reversed")]
    [InlineData("Voided")]
    public void PaymentStatus_ToString_Length_WithinVarcharFiftyLimit(string value)
    {
        var status = PaymentStatus.Create(value);
        Assert.True(status.ToString().Length <= 50,
            $"PaymentStatus.ToString() produced '{status}' ({status.ToString().Length} chars) — exceeds varchar(50) column limit.");
    }

    // ── PaymentChannel ──────────────────────────────────────────────────────

    [Fact]
    public void PaymentChannel_Counter_ToString_ReturnsValue()
        => Assert.Equal("Counter", PaymentChannel.Counter.ToString());

    [Fact]
    public void PaymentChannel_Import_ToString_ReturnsValue()
        => Assert.Equal("Import", PaymentChannel.Import.ToString());

    [Fact]
    public void PaymentChannel_Portal_ToString_ReturnsValue()
        => Assert.Equal("Portal", PaymentChannel.Portal.ToString());

    [Fact]
    public void PaymentChannel_Adjustment_ToString_ReturnsValue()
        => Assert.Equal("Adjustment", PaymentChannel.Adjustment.ToString());

    [Theory]
    [InlineData("Counter")]
    [InlineData("Import")]
    [InlineData("Portal")]
    [InlineData("Adjustment")]
    public void PaymentChannel_ToString_Length_WithinVarcharFiftyLimit(string value)
    {
        var channel = PaymentChannel.Create(value);
        Assert.True(channel.ToString().Length <= 50,
            $"PaymentChannel.ToString() produced '{channel}' ({channel.ToString().Length} chars) — exceeds varchar(50) column limit.");
    }

    // ── PaymentSource ───────────────────────────────────────────────────────

    [Fact]
    public void PaymentSource_Tenant_ToString_ReturnsValue()
        => Assert.Equal("Tenant", PaymentSource.Tenant.ToString());

    [Fact]
    public void PaymentSource_Landlord_ToString_ReturnsValue()
        => Assert.Equal("Landlord", PaymentSource.Landlord.ToString());

    [Fact]
    public void PaymentSource_Agency_ToString_ReturnsValue()
        => Assert.Equal("Agency", PaymentSource.Agency.ToString());

    [Fact]
    public void PaymentSource_SystemCorrection_ToString_ReturnsValue()
        => Assert.Equal("SystemCorrection", PaymentSource.SystemCorrection.ToString());

    [Theory]
    [InlineData("Tenant")]
    [InlineData("Landlord")]
    [InlineData("Agency")]
    [InlineData("SystemCorrection")]
    public void PaymentSource_ToString_Length_WithinVarcharFiftyLimit(string value)
    {
        var source = PaymentSource.Create(value);
        Assert.True(source.ToString().Length <= 50,
            $"PaymentSource.ToString() produced '{source}' ({source.ToString().Length} chars) — exceeds varchar(50) column limit.");
    }
}
