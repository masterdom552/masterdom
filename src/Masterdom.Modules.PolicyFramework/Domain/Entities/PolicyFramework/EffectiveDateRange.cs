using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class EffectiveDateRange : ValueObject
{
    private EffectiveDateRange(DateOnly startDate, DateOnly? endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public DateOnly StartDate { get; }

    public DateOnly? EndDate { get; }

    public static EffectiveDateRange Create(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new InvalidOperationException("Effective end date cannot be earlier than start date.");
        }

        return new EffectiveDateRange(startDate, endDate);
    }

    public bool Contains(DateOnly date)
    {
        return date >= StartDate && (!EndDate.HasValue || date <= EndDate.Value);
    }

    public bool Overlaps(EffectiveDateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisEnd = EndDate ?? DateOnly.MaxValue;
        var otherEnd = other.EndDate ?? DateOnly.MaxValue;

        return StartDate <= otherEnd && other.StartDate <= thisEnd;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
