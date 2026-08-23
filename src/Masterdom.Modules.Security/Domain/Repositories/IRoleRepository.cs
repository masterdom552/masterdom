using Masterdom.Core.Identity.Entities.Role;

namespace Masterdom.Modules.Security.Domain.Repositories;

public interface IRoleRepository
{
    void Add(Role role);

    Role? GetByCode(RoleCode roleCode);

    Role? GetById(RoleId roleId);
}
