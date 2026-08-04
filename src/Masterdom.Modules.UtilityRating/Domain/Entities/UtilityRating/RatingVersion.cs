using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class RatingVersion : ValueObject
{
    private RatingVersion(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static RatingVersion Initial => new(1);

    public static RatingVersion Create(int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException("Rating version must be greater than zero.");
        }

        return new RatingVersion(value);
    }

    public RatingVersion Next()
    {
        return new RatingVersion(Value + 1);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
