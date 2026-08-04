using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class RatingReference : ValueObject
{
    private RatingReference(IReadOnlyList<Guid> ratingIds)
    {
        RatingIds = ratingIds;
    }

    public IReadOnlyList<Guid> RatingIds { get; }

    public static RatingReference Create(IReadOnlyList<Guid> ratingIds)
    {
        ArgumentNullException.ThrowIfNull(ratingIds);

        if (ratingIds.Count == 0)
        {
            throw new InvalidOperationException("At least one rating reference is required.");
        }

        if (ratingIds.Any(x => x == Guid.Empty))
        {
            throw new InvalidOperationException("Rating reference contains an empty rating identifier.");
        }

        if (ratingIds.Distinct().Count() != ratingIds.Count)
        {
            throw new InvalidOperationException("Rating references cannot contain duplicates.");
        }

        return new RatingReference(ratingIds.ToArray());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var ratingId in RatingIds.OrderBy(x => x))
        {
            yield return ratingId;
        }
    }
}
