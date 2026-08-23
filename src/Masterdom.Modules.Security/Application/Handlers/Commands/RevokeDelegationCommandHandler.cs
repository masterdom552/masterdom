using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Modules.Security.Application.Handlers.Commands;

/// <summary>
/// Handler for RevokeDelegationCommand.
///
/// Maps command to domain operation via application service.
/// Catches domain exceptions and returns ExecutionResult.
/// </summary>
public sealed class RevokeDelegationCommandHandler : ICommandHandler<RevokeDelegationCommand, ExecutionResult<DelegatedAuthority>>
{
    private readonly IDelegationApplicationService _applicationService;

    public RevokeDelegationCommandHandler(IDelegationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<DelegatedAuthority> Handle(RevokeDelegationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var delegation = _applicationService.RevokeDelegation(command);
            return ExecutionResult<DelegatedAuthority>.Success(delegation);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<DelegatedAuthority>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<DelegatedAuthority>.Failure("conflict", ex.Message);
        }
    }
}
