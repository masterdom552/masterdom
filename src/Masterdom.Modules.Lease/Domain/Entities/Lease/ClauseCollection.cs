using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents immutable clause collection within lease versioning.
/// </summary>
public sealed class ClauseCollection : ValueObject
{
    private readonly IReadOnlyList<LeaseClause> _items;

    private ClauseCollection(IReadOnlyList<LeaseClause> items)
    {
        _items = items;
    }

    public IReadOnlyList<LeaseClause> Items => _items;

    public static ClauseCollection Create(IEnumerable<LeaseClause> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var materialized = items.ToList();
        if (materialized.Count == 0)
        {
            throw new InvalidOperationException("At least one lease clause is required.");
        }

        var duplicateCode = materialized
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateCode is not null)
        {
            throw new InvalidOperationException($"Duplicate clause code '{duplicateCode.Key}' is not allowed.");
        }

        return new ClauseCollection(materialized.AsReadOnly());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }
}
