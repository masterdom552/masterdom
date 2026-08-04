using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.People.Domain.Entities.Person.Events;

/// <summary>
/// Domain fact emitted when a person relationship is added.
/// </summary>
public sealed class RelationshipAddedDomainEvent : DomainEvent
{
    public RelationshipAddedDomainEvent(PersonId personId, PersonId relatedPersonId, string type)
    {
        ArgumentNullException.ThrowIfNull(personId);
        ArgumentNullException.ThrowIfNull(relatedPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        PersonId = personId;
        RelatedPersonId = relatedPersonId;
        Type = type.Trim();
    }

    public PersonId PersonId { get; }

    public PersonId RelatedPersonId { get; }

    public string Type { get; }
}
