using Masterdom.Modules.Security.Application.Commands;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Modules.Security.Application.Services;

public interface IIdentityAdministrationService
{
    RoleAggregate CreateRole(CreateRoleCommand command);
}
