using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents rent policy terms without billing execution behavior.
/// </summary>
public sealed class RentTerms : ValueObject
{
    private RentTerms(decimal monthlyRent, BillingFrequency billingFrequency, int rentDueDay, int gracePeriodDays)
    {
        MonthlyRent = monthlyRent;
        BillingFrequency = billingFrequency;
        RentDueDay = rentDueDay;
        GracePeriodDays = gracePeriodDays;
    }

    public decimal MonthlyRent { get; }

    public BillingFrequency BillingFrequency { get; }

    public int RentDueDay { get; }

    public int GracePeriodDays { get; }

    public static RentTerms Create(decimal monthlyRent, BillingFrequency billingFrequency, int rentDueDay, int gracePeriodDays)
    {
        ArgumentNullException.ThrowIfNull(billingFrequency);

        if (monthlyRent <= 0)
        {
            throw new InvalidOperationException("Monthly rent must be greater than zero.");
        }

        if (rentDueDay < 1 || rentDueDay > 31)
        {
            throw new InvalidOperationException("Rent due day must be between 1 and 31.");
        }

        if (gracePeriodDays < 0 || gracePeriodDays > 60)
        {
            throw new InvalidOperationException("Grace period days must be between 0 and 60.");
        }

        return new RentTerms(monthlyRent, billingFrequency, rentDueDay, gracePeriodDays);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MonthlyRent;
        yield return BillingFrequency;
        yield return RentDueDay;
        yield return GracePeriodDays;
    }
}
