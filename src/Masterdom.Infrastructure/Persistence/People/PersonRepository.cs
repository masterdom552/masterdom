using Masterdom.Core.Identifiers;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.People;

/// <summary>
/// EF Core repository implementation for people aggregates.
/// </summary>
public sealed class PersonRepository : IPersonRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PersonRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Person? GetById(PersonId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Persons
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .Include(x => x.EmergencyContacts)
            .Include(x => x.GovernmentDocuments)
            .Include(x => x.CommunicationPreferences)
            .Include(x => x.Relationships)
            .FirstOrDefault(x => x.Id == id);
    }

    public Person? GetByNumber(PersonNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return _dbContext.Persons
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .Include(x => x.EmergencyContacts)
            .Include(x => x.GovernmentDocuments)
            .Include(x => x.CommunicationPreferences)
            .Include(x => x.Relationships)
            .FirstOrDefault(x => x.Number == number);
    }

    public IReadOnlyCollection<Person> Search(string? numberContains, int take)
    {
        var effectiveTake = take <= 0 ? 50 : Math.Min(take, 200);

        var query = _dbContext.Persons
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .Include(x => x.EmergencyContacts)
            .Include(x => x.GovernmentDocuments)
            .Include(x => x.CommunicationPreferences)
            .Include(x => x.Relationships)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(numberContains))
        {
            query = query.Where(x =>
                x.Number.Value.Contains(numberContains.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return query.Take(effectiveTake).ToList();
    }

    public void Add(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        _dbContext.Persons.Add(person);
    }

    public void Update(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        _dbContext.Persons.Update(person);
    }
}
