using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security.Domain.Repositories;

namespace Masterdom.Modules.Security.Infrastructure;

public sealed class RoleRepository : IRoleRepository
{
    private readonly MasterdomDbContext _dbContext;

    public RoleRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        _dbContext.Roles.Add(role);
    }

    public Role? GetByCode(RoleCode roleCode)
    {
        ArgumentNullException.ThrowIfNull(roleCode);

        return _dbContext.Roles.FirstOrDefault(x => x.Code == roleCode);
    }
}
