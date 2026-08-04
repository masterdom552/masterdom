using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents an extensible business relationship from this person to another person identity.
/// </summary>
public sealed class PersonRelationship : ValueObject
{
    private PersonRelationship(PersonId relatedPersonId, string type, string? remarks)
    {
        RelatedPersonId = relatedPersonId;
        Type = type;
        Remarks = remarks;
    }

    public PersonId RelatedPersonId { get; }

    public string Type { get; }

    public string? Remarks { get; }

    public static PersonRelationship Create(PersonId relatedPersonId, string type, string? remarks = null)
    {
        ArgumentNullException.ThrowIfNull(relatedPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        return new PersonRelationship(
            relatedPersonId,
            type.Trim(),
            string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RelatedPersonId;
        yield return Type.ToUpperInvariant();
    }
}
