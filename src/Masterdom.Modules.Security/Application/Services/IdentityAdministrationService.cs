using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Support;
using Masterdom.Modules.Security.Domain.Repositories;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Modules.Security.Application.Services;

public sealed class IdentityAdministrationService : IIdentityAdministrationService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityAdministrationUnitOfWork _unitOfWork;

    public IdentityAdministrationService(
        IRoleRepository roleRepository,
        IIdentityAdministrationUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public RoleAggregate CreateRole(CreateRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var roleCode = RoleCode.Create(command.RoleCode);
        var roleName = RoleName.Create(command.RoleName);

        var existingRole = _roleRepository.GetByCode(roleCode);
        if (existingRole is not null)
        {
            throw new InvalidOperationException($"Role code '{roleCode.Value}' already exists.");
        }

        var role = RoleAggregate.Create(roleCode, roleName);

        _unitOfWork.Execute(() =>
        {
            _roleRepository.Add(role);
        });

        return role;
    }

    public RoleAggregate? GetRoleByCode(string roleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleCode);

        return _roleRepository.GetByCode(RoleCode.Create(roleCode));
    }
}
