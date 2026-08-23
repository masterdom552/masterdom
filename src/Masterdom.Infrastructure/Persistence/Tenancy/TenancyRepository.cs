using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Modules.Tenancy.Domain.Repositories;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Infrastructure.Persistence.Tenancy;

/// <summary>
/// EF Core repository implementation for tenancy aggregates.
/// </summary>
public sealed class TenancyRepository : ITenancyRepository
{
    private readonly MasterdomDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public TenancyRepository(MasterdomDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public void Add(TenancyAggregate tenancy)
    {
        ArgumentNullException.ThrowIfNull(tenancy);
        _dbContext.Tenancies.Add(tenancy);
    }

    public TenancyAggregate? GetById(TenancyId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return ApplyReadAccessFilter(_dbContext.Tenancies)
            .Include(x => x.Occupants)
            .FirstOrDefault(x => x.Id == id);
    }

    public bool HasActiveTenancyForUnit(UnitReference unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        return _dbContext.Tenancies
            .Any(x => x.Unit == unit && x.Status == TenancyStatus.Active);
    }

    public void Update(TenancyAggregate tenancy)
    {
        ArgumentNullException.ThrowIfNull(tenancy);
        _dbContext.Tenancies.Update(tenancy);
    }

    private IQueryable<TenancyAggregate> ApplyReadAccessFilter(IQueryable<TenancyAggregate> query)
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
        {
            return query.Where(_ => false);
        }

        if (currentUser.IsInherentSuperUser)
        {
            return query;
        }

        if (currentUser.IsInRole(MasterdomRoles.PropertyOwner) && currentUser.UserId.HasValue)
        {
            var ownedPropertyIds = _dbContext.Properties
                .Where(x => x.OwnerId == currentUser.UserId.Value)
                .Select(x => x.Id.Value);

            return query.Where(x => ownedPropertyIds.Contains(x.Property.PropertyId));
        }

        if (currentUser.IsInRole(MasterdomRoles.Manager))
        {
            var propertyScopes = currentUser.PropertyScopes.ToArray();
            if (propertyScopes.Length == 0)
            {
                return query.Where(_ => false);
            }

            return query.Where(x => propertyScopes.Contains(x.Property.PropertyId));
        }

        return query.Where(_ => false);
    }
}
