using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.CRM;

/// <summary>
/// EF Core repository implementation for CRM party aggregates.
/// </summary>
public sealed class PartyRepository : IPartyRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PartyRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Party? GetById(PartyId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return Query()
            .FirstOrDefault(x => x.Id == id);
    }

    public IReadOnlyCollection<Party> Search(string? displayNameContains, PartyType? partyType, int take)
    {
        var effectiveTake = take <= 0 ? 50 : Math.Min(take, 200);

        var query = Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(displayNameContains))
        {
            var filter = displayNameContains.Trim();
            query = query.Where(x => x.DisplayName.Contains(filter));
        }

        if (partyType is not null)
        {
            query = query.Where(x => x.PartyType == partyType);
        }

        return query
            .OrderBy(x => x.DisplayName)
            .Take(effectiveTake)
            .ToList();
    }

    public IReadOnlyCollection<Party> SearchByRole(PartyRoleType roleType, DateTime asOfUtc, int take)
    {
        ArgumentNullException.ThrowIfNull(roleType);

        var effectiveTake = take <= 0 ? 50 : Math.Min(take, 200);

        return Query()
            .AsEnumerable()
            .Where(x => x.RoleAssignments.Any(role => role.MatchesActiveRoleType(roleType, asOfUtc)))
            .OrderBy(x => x.DisplayName)
            .Take(effectiveTake)
            .ToList();
    }

    public void Add(Party party)
    {
        ArgumentNullException.ThrowIfNull(party);
        _dbContext.Parties.Add(party);
    }

    public void Update(Party party)
    {
        ArgumentNullException.ThrowIfNull(party);
        _dbContext.Parties.Update(party);
    }

    private IQueryable<Party> Query()
    {
        return _dbContext.Parties
            .Include(x => x.ContactMethods)
            .Include(x => x.Addresses)
            .Include(x => x.Relationships)
            .Include(x => x.RoleAssignments);
    }
}
