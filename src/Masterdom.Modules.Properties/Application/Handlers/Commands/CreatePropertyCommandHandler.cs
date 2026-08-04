using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

/// <summary>
/// Handles property-creation command orchestration.
/// </summary>
public sealed class CreatePropertyCommandHandler
    : ICommandHandler<CreatePropertyCommand, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public CreatePropertyCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(CreatePropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var property = _applicationService.CreateProperty(command);
            return ExecutionResult<Property>.Success(property);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Property>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Property>.Failure("conflict", ex.Message);
        }
    }
}
