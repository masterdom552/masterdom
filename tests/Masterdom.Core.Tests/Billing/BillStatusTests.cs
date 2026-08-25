using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing;

/// <summary>
/// Proves BillStatus.ToString() returns its Value, satisfying the
/// ValueObjectValueConverter contract and preventing varchar(50) overflow
/// on the bills.status column (PKG-BILL-STATUS-TOSTRING-PERSISTENCE-REPAIR).
/// </summary>
public sealed class BillStatusTests
{
    [Fact]
    public void ToString_Generated_ReturnsValue()
    {
        Assert.Equal("Generated", BillStatus.Generated.ToString());
    }

    [Fact]
    public void ToString_Draft_ReturnsValue()
    {
        Assert.Equal("Draft", BillStatus.Draft.ToString());
    }

    [Fact]
    public void ToString_Finalized_ReturnsValue()
    {
        Assert.Equal("Finalized", BillStatus.Finalized.ToString());
    }

    [Fact]
    public void ToString_Voided_ReturnsValue()
    {
        Assert.Equal("Voided", BillStatus.Voided.ToString());
    }

    [Theory]
    [InlineData("Generated")]
    [InlineData("Draft")]
    [InlineData("Finalized")]
    [InlineData("Voided")]
    public void ToString_Length_WithinVarcharFiftyLimit(string value)
    {
        var status = BillStatus.Create(value);
        Assert.True(status.ToString().Length <= 50,
            $"BillStatus.ToString() produced '{status}' ({status.ToString().Length} chars) — exceeds varchar(50) column limit.");
    }
}
