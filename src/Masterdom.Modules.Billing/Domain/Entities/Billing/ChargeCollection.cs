using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents immutable charge lines within a bill snapshot.
/// </summary>
public sealed class ChargeCollection : ValueObject
{
    private readonly IReadOnlyList<ChargeLine> _items;

    private ChargeCollection(IReadOnlyList<ChargeLine> items)
    {
        _items = items;
    }

    public IReadOnlyList<ChargeLine> Items => _items;

    public decimal TotalAmount => _items.Sum(x => x.Amount);

    public static ChargeCollection Create(IEnumerable<ChargeLine> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var materialized = items.ToList();
        if (materialized.Count == 0)
        {
            throw new InvalidOperationException("At least one charge is required to generate a bill.");
        }

        return new ChargeCollection(materialized.AsReadOnly());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
