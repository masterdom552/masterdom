using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

/// <summary>
/// Handles unit-removal command orchestration.
/// </summary>
public sealed class RemoveUnitCommandHandler
    : ICommandHandler<RemoveUnitCommand, ExecutionResult<bool>>
{
    private readonly IPropertyApplicationService _applicationService;

    public RemoveUnitCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var removed = _applicationService.RemoveUnit(command);
            if (!removed)
            {
                return ExecutionResult<bool>.Failure("not_found", "The requested unit was not found in the property.");
            }

            return ExecutionResult<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
