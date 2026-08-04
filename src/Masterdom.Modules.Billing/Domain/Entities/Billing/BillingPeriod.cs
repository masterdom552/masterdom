using Masterdom.Core.Financial.ValueObjects;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents billing period covered by a bill.
/// </summary>
public sealed class BillingPeriod : ValueObject
{
    private readonly FinancialPeriod _period;

    private BillingPeriod(FinancialPeriod period)
    {
        _period = period;
    }

    public DateOnly StartDate => _period.StartDate;

    public DateOnly EndDate => _period.EndDate;

    public static BillingPeriod Create(DateOnly startDate, DateOnly endDate)
    {
        return new BillingPeriod(FinancialPeriod.Create(startDate, endDate));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return _period;
    }
}
