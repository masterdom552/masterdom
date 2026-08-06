using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

public sealed class AddExistingUnitCommandHandler
    : ICommandHandler<AddExistingUnitCommand, ExecutionResult<Unit>>
{
    private readonly IPropertyApplicationService _applicationService;

    public AddExistingUnitCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Unit> Handle(AddExistingUnitCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var unit = _applicationService.AddExistingUnit(command);
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
