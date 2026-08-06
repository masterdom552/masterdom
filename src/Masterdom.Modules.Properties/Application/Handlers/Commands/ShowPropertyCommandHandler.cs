using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

public sealed class ShowPropertyCommandHandler
    : ICommandHandler<ShowPropertyCommand, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public ShowPropertyCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(ShowPropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var property = _applicationService.ShowProperty(command);
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
