namespace Masterdom.Core.Primitives;

/// <summary>
/// Represents the root of an aggregate.
///
/// Aggregate roots are responsible for maintaining
/// consistency within an aggregate boundary.
///
/// Domain event support will be added once the Events
/// infrastructure is implemented.
/// </summary>
/// <typeparam name="TId">
/// Strongly typed identifier.
/// </typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id)
        : base(id)
    {
    }
}
