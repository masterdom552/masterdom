using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Relationship;

/// <summary>
/// Represents the lifecycle status of a relationship.
/// </summary>
public sealed class RelationshipStatus : ValueObject
{
    public static readonly RelationshipStatus Active = new("Active");
    public static readonly RelationshipStatus Inactive = new("Inactive");
    public static readonly RelationshipStatus Archived = new("Archived");

    private RelationshipStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the relationship status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a relationship status.
    /// </summary>
    public static RelationshipStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new RelationshipStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
