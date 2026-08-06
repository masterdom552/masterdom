using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

public sealed class SetEffectivePeriodCommandHandler
    : ICommandHandler<SetEffectivePeriodCommand, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public SetEffectivePeriodCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(SetEffectivePeriodCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var property = _applicationService.SetEffectivePeriod(command);
            return ExecutionResult<Property>.Success(property);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Property>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Property>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
