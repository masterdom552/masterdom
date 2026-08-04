using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class MeterReference : ValueObject
{
    private MeterReference(Guid meterId)
    {
        MeterId = meterId;
    }

    public Guid MeterId { get; }

    public static MeterReference Create(Guid meterId)
    {
        if (meterId == Guid.Empty)
        {
            throw new ArgumentException("Meter reference cannot be empty.", nameof(meterId));
        }

        return new MeterReference(meterId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MeterId;
    }
}
