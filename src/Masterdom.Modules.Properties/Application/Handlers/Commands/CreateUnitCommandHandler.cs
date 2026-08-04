using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

/// <summary>
/// Handles unit-creation command orchestration.
/// </summary>
public sealed class CreateUnitCommandHandler
    : ICommandHandler<CreateUnitCommand, ExecutionResult<Unit>>
{
    private readonly IPropertyApplicationService _applicationService;

    public CreateUnitCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Unit> Handle(CreateUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var unit = _applicationService.CreateUnit(command);
            return ExecutionResult<Unit>.Success(unit);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Unit>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Unit>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
