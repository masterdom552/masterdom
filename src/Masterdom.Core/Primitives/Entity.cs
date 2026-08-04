namespace Masterdom.Core.Primitives;

/// <summary>
/// Represents an entity whose identity is defined by an identifier
/// rather than by the values of its properties.
/// </summary>
/// <typeparam name="TId">
/// The strongly typed identifier of the entity.
/// </typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the unique identity of the entity.
    /// </summary>
    public TId Id { get; }

    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
        => Equals(obj as Entity<TId>);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public static bool operator ==(
        Entity<TId>? left,
        Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(
        Entity<TId>? left,
        Entity<TId>? right)
        => !Equals(left, right);

    public override string ToString()
        => $"{GetType().Name} [{Id}]";
}
