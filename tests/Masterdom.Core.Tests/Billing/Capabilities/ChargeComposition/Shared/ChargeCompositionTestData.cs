using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;
using Masterdom.Modules.Billing.Application.Capabilities.Shared.Contracts;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Core.Tests.Billing.Capabilities.ChargeComposition.Shared;

internal static class ChargeCompositionTestData
{
    public static BillingContext CreateBillingContext()
    {
        return new BillingContext(
            BillingPeriod.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            BillingCycle.Monthly,
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 0, 0), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 8, 1, 0, 5, 0), DateTimeKind.Utc),
            PropertyReference.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            "corr-001");
    }

    public static BillabilityResolutionResult CreateBillabilityResolutionResult()
    {
        var included = new[]
        {
            new BillabilityCandidate(
                TenancyReference.Create(Guid.NewGuid()),
                LeaseReference.Create(Guid.NewGuid()),
                PropertyReference.Create(Guid.NewGuid()),
                Guid.NewGuid(),
                PersonReference.Create(PersonId.New()))
        };

        return new BillabilityResolutionResult(included, Array.Empty<ExcludedBillabilityCandidate>());
    }
}
