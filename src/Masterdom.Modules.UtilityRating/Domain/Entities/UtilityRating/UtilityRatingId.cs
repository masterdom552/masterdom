using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed record UtilityRatingId(Guid Value) : EntityId(Value)
{
    public static UtilityRatingId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static UtilityRatingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UtilityRatingId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
