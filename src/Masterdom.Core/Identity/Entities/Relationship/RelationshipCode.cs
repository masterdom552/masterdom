using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Relationship;

/// <summary>
/// Represents the business code of a relationship.
/// </summary>
public sealed class RelationshipCode : ValueObject
{
    private RelationshipCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the relationship code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a relationship code.
    /// </summary>
    public static RelationshipCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "Relationship code cannot exceed 50 characters.",
                nameof(value));
        }

        return new RelationshipCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(RelationshipCode code)
    {
        return code.Value;
    }
}
