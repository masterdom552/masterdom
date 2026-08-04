using Masterdom.Modules.People.Domain.Entities.Person;

namespace Masterdom.Modules.People.Domain.Repositories;

/// <summary>
/// Provides aggregate persistence boundary for people.
/// </summary>
public interface IPersonRepository
{
    Person? GetById(PersonId id);

    Person? GetByNumber(PersonNumber number);

    IReadOnlyCollection<Person> Search(string? numberContains, int take);

    void Add(Person person);

    void Update(Person person);
}
