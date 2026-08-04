using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationPeriod : ValueObject
{
    private OptimizationPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    public static OptimizationPeriod Create(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
        {
            throw new InvalidOperationException("Optimization period start date must be before end date.");
        }

        return new OptimizationPeriod(startDate, endDate);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
