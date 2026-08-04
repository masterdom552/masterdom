using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents lease clauses as a dedicated domain concept.
/// </summary>
public sealed class LeaseClauses : ValueObject
{
    private LeaseClauses(ClauseCollection collection)
    {
        Collection = collection;
    }

    public ClauseCollection Collection { get; }

    public static LeaseClauses Create(ClauseCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        return new LeaseClauses(collection);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Collection;
    }
}
