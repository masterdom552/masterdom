namespace Masterdom.Core.Primitives;

/// <summary>
/// Represents a strongly typed identifier for an entity.
/// </summary>
public abstract record EntityId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
