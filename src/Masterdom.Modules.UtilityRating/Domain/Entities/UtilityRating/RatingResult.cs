using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingResult : ValueObject
{
    private RatingResult(RatingBreakdown breakdown, DateTime generatedAtUtc)
    {
        Breakdown = breakdown;
        GeneratedAtUtc = generatedAtUtc;
    }

    public RatingBreakdown Breakdown { get; }

    public DateTime GeneratedAtUtc { get; }

    public static RatingResult Create(RatingBreakdown breakdown, DateTime generatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(breakdown);

        if (generatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Rating result timestamp must be UTC.");
        }

        return new RatingResult(breakdown, generatedAtUtc);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Breakdown;
        yield return GeneratedAtUtc;
    }
}
