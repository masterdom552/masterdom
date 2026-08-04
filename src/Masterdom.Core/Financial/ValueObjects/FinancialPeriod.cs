using Masterdom.Core.Primitives;

namespace Masterdom.Core.Financial.ValueObjects;

public sealed class FinancialPeriod : ValueObject
{
    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    private FinancialPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static FinancialPeriod Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be on or after the start date.", nameof(endDate));

        return new FinancialPeriod(startDate, endDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
