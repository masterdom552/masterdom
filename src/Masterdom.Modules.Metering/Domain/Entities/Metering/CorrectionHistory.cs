using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering;

public sealed class CorrectionHistory : ValueObject
{
    private readonly IReadOnlyList<ReadingCorrection> _items;

    private CorrectionHistory(IReadOnlyList<ReadingCorrection> items)
    {
        _items = items;
    }

    public static CorrectionHistory Empty => new([]);

    public IReadOnlyList<ReadingCorrection> Items => _items;

    public CorrectionHistory Add(ReadingCorrection correction)
    {
        ArgumentNullException.ThrowIfNull(correction);

        var next = _items.ToList();
        next.Add(correction);
        return new CorrectionHistory(next.AsReadOnly());
    }

    public static CorrectionHistory Create(IEnumerable<ReadingCorrection> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new CorrectionHistory(items.ToList().AsReadOnly());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
