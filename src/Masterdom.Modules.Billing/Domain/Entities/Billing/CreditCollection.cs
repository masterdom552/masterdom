using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents immutable credits collection.
/// </summary>
public sealed class CreditCollection : ValueObject
{
    private readonly IReadOnlyList<CreditLine> _items;

    private CreditCollection(IReadOnlyList<CreditLine> items)
    {
        _items = items;
    }

    public static CreditCollection Empty => new([]);

    public IReadOnlyList<CreditLine> Items => _items;

    public decimal TotalAmount => _items.Sum(x => x.Amount);

    public CreditCollection Add(CreditLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var next = _items.ToList();
        next.Add(line);
        return new CreditCollection(next.AsReadOnly());
    }

    public static CreditCollection Create(IEnumerable<CreditLine> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new CreditCollection(items.ToList().AsReadOnly());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
