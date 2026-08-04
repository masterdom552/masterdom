using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Application.Support;

namespace Masterdom.Modules.Tenancy.Application.Handlers.Commands;

public sealed class RemoveOccupantCommandHandler : ICommandHandler<RemoveOccupantCommand, ExecutionResult<bool>>
{
    private readonly ITenancyApplicationService _applicationService;

    public RemoveOccupantCommandHandler(ITenancyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveOccupantCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var removed = _applicationService.RemoveOccupant(command);
            return ExecutionResult<bool>.Success(removed);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<bool>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("conflict", ex.Message);
        }
    }
}
