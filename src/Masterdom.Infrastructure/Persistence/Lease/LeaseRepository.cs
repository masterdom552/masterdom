using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Lease.Domain.Repositories;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Infrastructure.Persistence.Lease;

/// <summary>
/// EF Core repository implementation for lease aggregates.
/// </summary>
public sealed class LeaseRepository : ILeaseRepository
{
    private readonly MasterdomDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public LeaseRepository(MasterdomDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public void Add(LeaseAggregate lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        _dbContext.Leases.Add(lease);
    }

    public LeaseAggregate? GetById(LeaseId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return ApplyReadAccessFilter(_dbContext.Leases)
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.Id == id);
    }

    public LeaseAggregate? GetByNumber(LeaseNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return ApplyReadAccessFilter(_dbContext.Leases)
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.Number == number);
    }

    public bool HasActiveLeaseForTenancy(TenancyReference tenancy)
    {
        ArgumentNullException.ThrowIfNull(tenancy);

        return _dbContext.Leases
            .Any(x => x.Tenancy == tenancy && x.Status == LeaseStatus.Active);
    }

    public void Update(LeaseAggregate lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        _dbContext.Leases.Update(lease);
    }

    private IQueryable<LeaseAggregate> ApplyReadAccessFilter(IQueryable<LeaseAggregate> query)
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
                .Select(x => x.Id);

            return query.Where(x => ownedPropertyIds.Contains(x.Property));
        }

        if (currentUser.IsInRole(MasterdomRoles.Manager))
        {
            var propertyScopes = currentUser.PropertyScopes.ToArray();
            if (propertyScopes.Length == 0)
            {
                return query.Where(_ => false);
            }

            return query.Where(x => propertyScopes.Contains(x.Property));
        }

        return query.Where(_ => false);
    }
}
