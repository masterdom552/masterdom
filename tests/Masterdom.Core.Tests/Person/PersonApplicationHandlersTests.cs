using Masterdom.Core.Identifiers;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Handlers.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;
using Masterdom.Modules.People.Domain.Repositories;
using PersonAggregate = Masterdom.Modules.People.Domain.Entities.Person.Person;

namespace Masterdom.Core.Tests.Person;

public sealed class PersonApplicationHandlersTests
{
    [Fact]
    public void RenamePersonHandler_ShouldRenameThroughAggregateBehavior()
    {
        var aggregate = PersonAggregate.Create(
            PersonNumber.Create("APP-P-01"),
            PersonName.Create("Initial", "Name"),
            Gender.Other);

        var repository = new InMemoryPersonRepository(aggregate);
        var unitOfWork = new SpyUnitOfWork();
        var orchestrator = new SpyPlatformOrchestrator();

        var service = new PersonApplicationService(repository, unitOfWork, orchestrator);
        var handler = new RenamePersonCommandHandler(service);

        var result = handler.Handle(new RenamePersonCommand(aggregate.Id, PersonName.Create("Renamed", "Person")));

        Assert.True(result.IsSuccess);
        Assert.Equal("Renamed", aggregate.Name.FirstName);
        Assert.Equal(1, unitOfWork.ExecuteCount);
        Assert.Equal(1, orchestrator.MutationCount);
    }

    [Fact]
    public void AddIdentityDocumentHandler_ShouldRespectDuplicateTypeRule()
    {
        var aggregate = PersonAggregate.Create(
            PersonNumber.Create("APP-P-02"),
            PersonName.Create("Doc", "Holder"),
            Gender.Other);

        aggregate.AddGovernmentDocument(GovernmentDocument.Create("Passport", "X-1"));

        var repository = new InMemoryPersonRepository(aggregate);
        var service = new PersonApplicationService(repository, new SpyUnitOfWork(), new SpyPlatformOrchestrator());
        var handler = new AddIdentityDocumentCommandHandler(service);

        var result = handler.Handle(new AddIdentityDocumentCommand(aggregate.Id, GovernmentDocument.Create("Passport", "X-2")));

        Assert.False(result.IsSuccess);
        Assert.Equal("domain_rule_violation", result.ErrorCode);
    }

    private sealed class InMemoryPersonRepository : IPersonRepository
    {
        private readonly Dictionary<Guid, PersonAggregate> _people;

        public InMemoryPersonRepository(params PersonAggregate[] people)
        {
            _people = people.ToDictionary(x => x.Id.Value, x => x);
        }

        public PersonAggregate? GetById(PersonId id)
        {
            return _people.TryGetValue(id.Value, out var person) ? person : null;
        }

        public PersonAggregate? GetByNumber(PersonNumber number)
        {
            return _people.Values.FirstOrDefault(x => x.Number == number);
        }

        public IReadOnlyCollection<PersonAggregate> Search(string? numberContains, int take)
        {
            var effectiveTake = take <= 0 ? 50 : take;
            var query = _people.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(numberContains))
            {
                query = query.Where(x => x.Number.Value.Contains(numberContains, StringComparison.OrdinalIgnoreCase));
            }

            return query.Take(effectiveTake).ToList();
        }

        public void Add(PersonAggregate person)
        {
            _people[person.Id.Value] = person;
        }

        public void Update(PersonAggregate person)
        {
            _people[person.Id.Value] = person;
        }
    }

    private sealed class SpyUnitOfWork : IPersonUnitOfWork
    {
        public int ExecuteCount { get; private set; }

        public void Execute(Action operation)
        {
            ExecuteCount++;
            operation();
        }
    }

    private sealed class SpyPlatformOrchestrator : IPersonPlatformOrchestrator
    {
        public int MutationCount { get; private set; }

        public void OnPersonMutated(PersonAggregate person, string operationName)
        {
            MutationCount++;
        }
    }
}
