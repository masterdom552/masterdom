using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

/// <summary>
/// Handles property-rename command orchestration.
/// </summary>
public sealed class RenamePropertyCommandHandler
    : ICommandHandler<RenamePropertyCommand, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public RenamePropertyCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(RenamePropertyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var property = _applicationService.RenameProperty(command);
            return ExecutionResult<Property>.Success(property);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Property>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Property>.Failure("not_found", ex.Message);
        }
    }
}
