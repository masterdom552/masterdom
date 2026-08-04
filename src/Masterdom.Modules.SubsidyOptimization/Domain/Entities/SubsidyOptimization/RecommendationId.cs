using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class RecommendationId : ValueObject
{
    private RecommendationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static RecommendationId New()
    {
        return new RecommendationId(Guid.CreateVersion7());
    }

    public static RecommendationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RecommendationId cannot be empty.", nameof(value));
        }

        return new RecommendationId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
