using Masterdom.Core.Primitives;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

public sealed class ConsumptionReference : ValueObject
{
    private ConsumptionReference(Guid readingId, decimal consumptionValue)
    {
        ReadingId = readingId;
        ConsumptionValue = consumptionValue;
    }

    public Guid ReadingId { get; }

    public decimal ConsumptionValue { get; }

    public static ConsumptionReference Create(Guid readingId, decimal consumptionValue)
    {
        if (readingId == Guid.Empty)
        {
            throw new ArgumentException("Consumption reference reading id cannot be empty.", nameof(readingId));
        }

        if (consumptionValue < 0)
        {
            throw new InvalidOperationException("Consumption value cannot be negative.");
        }

        return new ConsumptionReference(readingId, consumptionValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ReadingId;
        yield return ConsumptionValue;
    }
}
