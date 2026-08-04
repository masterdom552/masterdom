using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.People.Domain.Entities.Person.Events;

/// <summary>
/// Domain fact emitted when an identity document is added.
/// </summary>
public sealed class IdentityDocumentAddedDomainEvent : DomainEvent
{
    public IdentityDocumentAddedDomainEvent(PersonId personId, string documentType, string documentNumber)
    {
        ArgumentNullException.ThrowIfNull(personId);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        PersonId = personId;
        DocumentType = documentType.Trim();
        DocumentNumber = documentNumber.Trim();
    }

    public PersonId PersonId { get; }

    public string DocumentType { get; }

    public string DocumentNumber { get; }
}
