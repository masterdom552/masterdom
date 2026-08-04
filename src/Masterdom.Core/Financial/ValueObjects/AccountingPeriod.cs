using Masterdom.Core.Primitives;

namespace Masterdom.Core.Financial.ValueObjects;

public sealed class AccountingPeriod : ValueObject
{
    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    private AccountingPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static AccountingPeriod Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be on or after the start date.", nameof(endDate));

        return new AccountingPeriod(startDate, endDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
