using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Repositories;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Infrastructure.Persistence.Property;

/// <summary>
/// EF Core repository implementation for property aggregates.
/// </summary>
public sealed class PropertyRepository : IPropertyRepository
{
    private readonly MasterdomDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public PropertyRepository(MasterdomDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public PropertyAggregate? GetById(PropertyId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return ApplyReadAccessFilter(_dbContext.Properties)
            .Include(x => x.Units)
            .FirstOrDefault(x => x.Id == id);
    }

    public PropertyAggregate? GetByCode(PropertyCode code)
    {
        ArgumentNullException.ThrowIfNull(code);

        return ApplyReadAccessFilter(_dbContext.Properties)
            .Include(x => x.Units)
            .FirstOrDefault(x => x.Code == code);
    }

    public IReadOnlyCollection<Unit> ListUnits(PropertyId propertyId)
    {
        ArgumentNullException.ThrowIfNull(propertyId);

        var property = ApplyReadAccessFilter(_dbContext.Properties)
            .Include(x => x.Units)
            .FirstOrDefault(x => x.Id == propertyId);

        return property?.Units.ToList() ?? [];
    }

    public IReadOnlyCollection<PropertyAggregate> Search(string? codeContains, int take)
    {
        var effectiveTake = take <= 0 ? 50 : Math.Min(take, 200);

        var query = ApplyReadAccessFilter(_dbContext.Properties)
            .Include(x => x.Units)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(codeContains))
        {
            query = query.Where(x =>
                x.Code.Value.Contains(codeContains.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return query
            .Take(effectiveTake)
            .ToList();
    }

    public IReadOnlyCollection<PropertyAggregate> ListOwnedBy(Guid ownerId)
    {
        return _dbContext.Properties
            .Where(x => x.OwnerId == ownerId)
            .ToList();
    }

    public void Add(PropertyAggregate property)
    {
        ArgumentNullException.ThrowIfNull(property);
        _dbContext.Properties.Add(property);
    }

    public void Update(PropertyAggregate property)
    {
        ArgumentNullException.ThrowIfNull(property);
        _dbContext.Properties.Update(property);
    }

    private IQueryable<PropertyAggregate> ApplyReadAccessFilter(IQueryable<PropertyAggregate> query)
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
            var userId = currentUser.UserId.Value;
            return query.Where(x => x.OwnerId == userId);
        }

        if (currentUser.IsInRole(MasterdomRoles.Manager))
        {
            var propertyScopes = currentUser.PropertyScopes.ToArray();
            if (propertyScopes.Length == 0)
            {
                return query.Where(_ => false);
            }

            return query.Where(x => propertyScopes.Contains(x.Id.Value));
        }

        return query.Where(_ => false);
    }
}
