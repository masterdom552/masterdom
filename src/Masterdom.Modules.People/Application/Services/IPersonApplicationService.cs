using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Queries;

namespace Masterdom.Modules.People.Application.Services;

/// <summary>
/// Defines application orchestration boundary for people use-cases.
/// </summary>
public interface IPersonApplicationService
{
    Person CreatePerson(CreatePersonCommand command);

    Person RenamePerson(RenamePersonCommand command);

    Person ChangeStatus(ChangePersonStatusCommand command);

    Person AddContact(AddContactCommand command);

    bool RemoveContact(RemoveContactCommand command);

    Person AddIdentityDocument(AddIdentityDocumentCommand command);

    Person AddRelationship(AddRelationshipCommand command);

    Person? GetPerson(GetPersonByIdQuery query);

    Person? GetPersonByNumber(GetPersonByNumberQuery query);

    IReadOnlyCollection<Person> SearchPeople(SearchPeopleQuery query);
}
