using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.People.Domain.Entities.Person.Events;

/// <summary>
/// Domain fact emitted when a contact is added.
/// </summary>
public sealed class ContactAddedDomainEvent : DomainEvent
{
    public ContactAddedDomainEvent(PersonId personId, string contactType, string contactValue)
    {
        ArgumentNullException.ThrowIfNull(personId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactType);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactValue);

        PersonId = personId;
        ContactType = contactType.Trim();
        ContactValue = contactValue.Trim();
    }

    public PersonId PersonId { get; }

    public string ContactType { get; }

    public string ContactValue { get; }
}
