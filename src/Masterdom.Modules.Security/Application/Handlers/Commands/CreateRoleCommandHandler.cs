using Masterdom.Core.Security;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Support;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Modules.Security.Application.Handlers.Commands;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, ExecutionResult<RoleAggregate>>
{
    private const string Operation = "identity.roles.create";

    private readonly IIdentityAdministrationService _identityAdministrationService;
    private readonly IPropertyCapabilityAuthorizationService _authorizationService;

    public CreateRoleCommandHandler(
        IIdentityAdministrationService identityAdministrationService,
        IPropertyCapabilityAuthorizationService authorizationService)
    {
        _identityAdministrationService = identityAdministrationService
            ?? throw new ArgumentNullException(nameof(identityAdministrationService));
        _authorizationService = authorizationService
            ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public ExecutionResult<RoleAggregate> Handle(CreateRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var authorizationResult = _authorizationService.Authorize(new AuthorizationContext(Operation));
        if (!authorizationResult.IsAllowed)
        {
            return ExecutionResult<RoleAggregate>.Failure(
                authorizationResult.ErrorCode,
                authorizationResult.ErrorMessage ?? "The request is not authorized.");
        }

        try
        {
            var role = _identityAdministrationService.CreateRole(command);
            return ExecutionResult<RoleAggregate>.Success(role);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<RoleAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<RoleAggregate>.Failure("conflict", ex.Message);
        }
    }
}
