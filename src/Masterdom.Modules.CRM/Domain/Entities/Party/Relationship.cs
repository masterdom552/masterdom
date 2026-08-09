using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents a directional relationship from one party to another.
/// </summary>
public sealed class Relationship : ValueObject
{
    private Relationship(PartyId relatedPartyId, RelationshipType type, bool allowsSelfReference)
    {
        RelatedPartyId = relatedPartyId;
        Type = type;
        AllowsSelfReference = allowsSelfReference;
    }

    public PartyId RelatedPartyId { get; }

    public RelationshipType Type { get; }

    public bool AllowsSelfReference { get; }

    public static Relationship Create(PartyId relatedPartyId, string type, bool allowsSelfReference = false)
    {
        return Create(relatedPartyId, RelationshipType.Create(type), allowsSelfReference);
    }

    public static Relationship Create(PartyId relatedPartyId, RelationshipType type, bool allowsSelfReference = false)
    {
        ArgumentNullException.ThrowIfNull(relatedPartyId);
        ArgumentNullException.ThrowIfNull(type);

        return new Relationship(relatedPartyId, type, allowsSelfReference);
    }

    public bool Matches(Relationship other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return RelatedPartyId == other.RelatedPartyId && Type == other.Type;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RelatedPartyId;
        yield return Type;
    }
}
