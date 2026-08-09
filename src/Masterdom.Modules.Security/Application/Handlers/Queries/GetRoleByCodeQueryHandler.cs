using Masterdom.Core.Security;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Support;
using RoleAggregate = Masterdom.Core.Identity.Entities.Role.Role;

namespace Masterdom.Modules.Security.Application.Handlers.Queries;

public sealed class GetRoleByCodeQueryHandler : IQueryHandler<GetRoleByCodeQuery, ExecutionResult<RoleAggregate>>
{
    private const string Operation = "identity.roles.read.by-code";

    private readonly IIdentityAdministrationService _identityAdministrationService;
    private readonly IPropertyCapabilityAuthorizationService _authorizationService;

    public GetRoleByCodeQueryHandler(
        IIdentityAdministrationService identityAdministrationService,
        IPropertyCapabilityAuthorizationService authorizationService)
    {
        _identityAdministrationService = identityAdministrationService
            ?? throw new ArgumentNullException(nameof(identityAdministrationService));
        _authorizationService = authorizationService
            ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public ExecutionResult<RoleAggregate> Handle(GetRoleByCodeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var authorizationResult = _authorizationService.Authorize(new AuthorizationContext(Operation));
        if (!authorizationResult.IsAllowed)
        {
            return ExecutionResult<RoleAggregate>.Failure(
                authorizationResult.ErrorCode,
                authorizationResult.ErrorMessage ?? "The request is not authorized.");
        }

        try
        {
            var role = _identityAdministrationService.GetRoleByCode(query.RoleCode);
            return role is null
                ? ExecutionResult<RoleAggregate>.Failure("not_found", $"Role code '{query.RoleCode}' was not found.")
                : ExecutionResult<RoleAggregate>.Success(role);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<RoleAggregate>.Failure("validation_failed", ex.Message);
        }
    }
}
