using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents immutable adjustment lines.
/// </summary>
public sealed class AdjustmentCollection : ValueObject
{
    private readonly IReadOnlyList<AdjustmentLine> _items;

    private AdjustmentCollection(IReadOnlyList<AdjustmentLine> items)
    {
        _items = items;
    }

    public static AdjustmentCollection Empty => new([]);

    public IReadOnlyList<AdjustmentLine> Items => _items;

    public decimal TotalAmount => _items.Sum(x => x.Amount);

    public AdjustmentCollection Add(AdjustmentLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var next = _items.ToList();
        next.Add(line);
        return new AdjustmentCollection(next.AsReadOnly());
    }

    public static AdjustmentCollection Create(IEnumerable<AdjustmentLine> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new AdjustmentCollection(items.ToList().AsReadOnly());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
