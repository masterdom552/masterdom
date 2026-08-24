using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core repository implementation for DelegatedAuthority.
/// </summary>
public sealed class DelegatedAuthorityRepository : IDelegatedAuthorityRepository
{
    private readonly MasterdomDbContext _dbContext;

    public DelegatedAuthorityRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<DelegatedAuthority?> GetByIdAsync(DelegatedAuthorityId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _dbContext.DelegatedAuthorities
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(
        Guid delegatedToUserId,
        DateTime utcNow)
    {
        return await _dbContext.DelegatedAuthorities
            .Where(x => x.DelegatedToUserId == UserId.From(delegatedToUserId))
            .Where(x => x.Status != DelegatedAuthorityStatus.Revoked)
            .Where(x => x.EffectiveFromUtc <= utcNow)
            .Where(x => x.EffectiveToUtc == null || x.EffectiveToUtc >= utcNow)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<DelegatedAuthority>> GetDelegationsByDelegatorAsync(
        Guid delegatorUserId)
    {
        return await _dbContext.DelegatedAuthorities
            .Where(x => x.DelegatorUserId == UserId.From(delegatorUserId))
            .ToListAsync();
    }

    public void Add(DelegatedAuthority delegation)
    {
        ArgumentNullException.ThrowIfNull(delegation);

        _dbContext.DelegatedAuthorities.Add(delegation);
    }

    public void Update(DelegatedAuthority delegation)
    {
        ArgumentNullException.ThrowIfNull(delegation);

        _dbContext.DelegatedAuthorities.Update(delegation);
    }
}
