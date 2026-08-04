using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;
using Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Shared;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Contracts;

public sealed class ChargeCompositionRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeFields()
    {
        var billingContext = ChargeCompositionTestData.CreateBillingContext();
        var billability = ChargeCompositionTestData.CreateBillabilityResolutionResult();

        var request = new ChargeCompositionRequest(billingContext, billability);

        Assert.Equal(billingContext, request.BillingContext);
        Assert.Equal(billability, request.BillabilityResolutionResult);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBillingContextIsNull()
    {
        var billability = ChargeCompositionTestData.CreateBillabilityResolutionResult();

        Assert.Throws<ArgumentNullException>(() => new ChargeCompositionRequest(null!, billability));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBillabilityResolutionResultIsNull()
    {
        var billingContext = ChargeCompositionTestData.CreateBillingContext();

        Assert.Throws<ArgumentNullException>(() => new ChargeCompositionRequest(billingContext, null!));
    }

    [Fact]
    public void Contract_ShouldBeImmutableByPublicApi()
    {
        var mutablePropertyExists = typeof(ChargeCompositionRequest)
            .GetProperties()
            .Any(x => x.SetMethod is not null && x.SetMethod.IsPublic);

        Assert.False(mutablePropertyExists);
    }

    [Fact]
    public void Record_ShouldSupportValueEquality()
    {
        var billingContext = ChargeCompositionTestData.CreateBillingContext();
        var billability = ChargeCompositionTestData.CreateBillabilityResolutionResult();

        var left = new ChargeCompositionRequest(billingContext, billability);
        var right = new ChargeCompositionRequest(billingContext, billability);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
