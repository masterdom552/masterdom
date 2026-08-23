using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Support;

namespace Masterdom.Modules.Security.Application.Handlers.Commands;

/// <summary>
/// Handler for CreateDelegationCommand.
///
/// Maps command to domain operation via application service.
/// Catches domain exceptions and returns ExecutionResult.
/// </summary>
public sealed class CreateDelegationCommandHandler : ICommandHandler<CreateDelegationCommand, ExecutionResult<DelegatedAuthority>>
{
    private readonly IDelegationApplicationService _applicationService;

    public CreateDelegationCommandHandler(IDelegationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<DelegatedAuthority> Handle(CreateDelegationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Note: Synchronously waiting for async Application service.
            // Future architectural decision may convert entire handler chain to async.
            var delegation = _applicationService.CreateDelegationAsync(command, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
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
