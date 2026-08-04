using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Relationship;

/// <summary>
/// Represents the unique identifier of a relationship.
/// </summary>
public sealed record RelationshipId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new relationship identifier.
    /// </summary>
    public static RelationshipId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a relationship identifier from an existing Guid.
    /// </summary>
    public static RelationshipId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "RelationshipId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into a relationship identifier.
    /// </summary>
    public static RelationshipId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
