using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.People.Domain.Entities.Person.Events;

/// <summary>
/// Domain fact emitted when person details are updated.
/// </summary>
public sealed class PersonUpdatedDomainEvent : DomainEvent
{
    public PersonUpdatedDomainEvent(PersonId personId)
    {
        ArgumentNullException.ThrowIfNull(personId);
        PersonId = personId;
    }

    public PersonId PersonId { get; }
}
