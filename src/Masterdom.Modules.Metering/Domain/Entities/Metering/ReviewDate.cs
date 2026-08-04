using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class ReviewDate : ValueObject
{
    private ReviewDate(DateTime valueUtc)
    {
        ValueUtc = valueUtc;
    }

    public DateTime ValueUtc { get; }

    public static ReviewDate Create(DateTime valueUtc)
    {
        return new ReviewDate(DateTime.SpecifyKind(valueUtc, DateTimeKind.Utc));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ValueUtc;
    }
}
