using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles deactivate-party-role command orchestration.
/// </summary>
public sealed class DeactivatePartyRoleCommandHandler : ICommandHandler<DeactivatePartyRoleCommand, ExecutionResult<bool>>
{
    private readonly IPartyApplicationService _applicationService;

    public DeactivatePartyRoleCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(DeactivatePartyRoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var deactivated = _applicationService.DeactivatePartyRole(command);
            return deactivated
                ? ExecutionResult<bool>.Success(true)
                : ExecutionResult<bool>.Failure("not_found", "The role assignment was not found.");
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
