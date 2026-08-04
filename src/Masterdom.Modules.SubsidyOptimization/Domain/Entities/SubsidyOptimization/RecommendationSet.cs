using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class RecommendationSet : ValueObject
{
    private RecommendationSet(IReadOnlyList<OptimizationRecommendation> items)
    {
        Items = items;
    }

    public IReadOnlyList<OptimizationRecommendation> Items { get; }

    public static RecommendationSet Create(IReadOnlyCollection<OptimizationRecommendation> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = items
            .OrderBy(x => x.Priority.Rank)
            .ThenBy(x => x.RecommendationId.Value)
            .ToList();

        if (list.Count == 0)
        {
            throw new InvalidOperationException("At least one recommendation is required.");
        }

        if (list.Select(x => x.RecommendationId).Distinct().Count() != list.Count)
        {
            throw new InvalidOperationException("Duplicate recommendation identifiers are not allowed.");
        }

        return new RecommendationSet(list);
    }

    public RecommendationSet Archive(RecommendationId recommendationId, string reason, DateTime archivedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(recommendationId);

        var index = Items.ToList().FindIndex(x => x.RecommendationId == recommendationId);
        if (index < 0)
        {
            throw new InvalidOperationException("Recommendation was not found.");
        }

        var clone = Items.ToList();
        clone[index] = clone[index].Archive(reason, archivedAtUtc);
        return new RecommendationSet(clone);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in Items)
        {
            yield return item;
        }
    }
}
