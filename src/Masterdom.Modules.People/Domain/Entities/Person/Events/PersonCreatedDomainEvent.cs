using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.People.Domain.Entities.Person.Events;

/// <summary>
/// Domain fact emitted when a person is created.
/// </summary>
public sealed class PersonCreatedDomainEvent : DomainEvent
{
    public PersonCreatedDomainEvent(PersonId personId, PersonNumber personNumber)
    {
        ArgumentNullException.ThrowIfNull(personId);
        ArgumentNullException.ThrowIfNull(personNumber);

        PersonId = personId;
        PersonNumber = personNumber;
    }

    public PersonId PersonId { get; }

    public PersonNumber PersonNumber { get; }
}
