namespace Masterdom.Modules.Security.Application.Commands;

public sealed record CreateRoleCommand(
    string RoleCode,
    string RoleName);
