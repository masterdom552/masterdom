using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingPeriod : ValueObject
{
    private RatingPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    public static RatingPeriod Create(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
        {
            throw new InvalidOperationException("Rating period start date must be before end date.");
        }

        return new RatingPeriod(startDate, endDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
