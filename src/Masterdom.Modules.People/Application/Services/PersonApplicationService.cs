using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Application.Support;
using Masterdom.Modules.People.Domain.Repositories;

namespace Masterdom.Modules.People.Application.Services;

/// <summary>
/// Orchestrates people use-cases through aggregate APIs.
/// </summary>
public sealed class PersonApplicationService : IPersonApplicationService
{
    private readonly IPersonRepository _repository;
    private readonly IPersonUnitOfWork _unitOfWork;
    private readonly IPersonPlatformOrchestrator _platformOrchestrator;

    public PersonApplicationService(
        IPersonRepository repository,
        IPersonUnitOfWork unitOfWork,
        IPersonPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public Person CreatePerson(CreatePersonCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_repository.GetByNumber(command.Number) is not null)
        {
            throw new InvalidOperationException($"Person number '{command.Number.Value}' already exists.");
        }

        var person = Person.Create(command.Number, command.Name, command.Gender);

        _unitOfWork.Execute(() =>
        {
            _repository.Add(person);
        });

        _platformOrchestrator.OnPersonMutated(person, "CreatePerson");

        return person;
    }

    public Person RenamePerson(RenamePersonCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);
        person.Rename(command.Name);

        PersistAndCoordinate(person, "RenamePerson");

        return person;
    }

    public Person ChangeStatus(ChangePersonStatusCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);

        if (command.Status == PersonStatus.Active)
        {
            person.Activate();
        }
        else if (command.Status == PersonStatus.Inactive)
        {
            person.Deactivate();
        }
        else if (command.Status == PersonStatus.Archived)
        {
            person.Archive();
        }
        else
        {
            throw new InvalidOperationException($"Unsupported person status '{command.Status}'.");
        }

        PersistAndCoordinate(person, "ChangeStatus");

        return person;
    }

    public Person AddContact(AddContactCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);
        person.AddContact(command.Contact);

        PersistAndCoordinate(person, "AddContact");

        return person;
    }

    public bool RemoveContact(RemoveContactCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);
        var removed = person.RemoveContact(command.Contact);

        if (!removed)
        {
            return false;
        }

        PersistAndCoordinate(person, "RemoveContact");

        return true;
    }

    public Person AddIdentityDocument(AddIdentityDocumentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);
        person.AddGovernmentDocument(command.Document);

        PersistAndCoordinate(person, "AddIdentityDocument");

        return person;
    }

    public Person AddRelationship(AddRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var person = GetRequiredPerson(command.PersonId);
        person.AddRelationship(command.Relationship);

        PersistAndCoordinate(person, "AddRelationship");

        return person;
    }

    public Person? GetPerson(GetPersonByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.PersonId);
    }

    public Person? GetPersonByNumber(GetPersonByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetByNumber(query.Number);
    }

    public IReadOnlyCollection<Person> SearchPeople(SearchPeopleQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.Search(query.NumberContains, query.Take);
    }

    private Person GetRequiredPerson(PersonId personId)
    {
        var person = _repository.GetById(personId);
        if (person is null)
        {
            throw new InvalidOperationException($"Person '{personId}' was not found.");
        }

        return person;
    }

    private void PersistAndCoordinate(Person person, string operationName)
    {
        _unitOfWork.Execute(() =>
        {
            _repository.Update(person);
        });

        _platformOrchestrator.OnPersonMutated(person, operationName);
    }
}
