using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Commands;

public sealed class RemoveMetadataCommandHandler
    : ICommandHandler<RemoveMetadataCommand, ExecutionResult<bool>>
{
    private readonly IPropertyApplicationService _applicationService;

    public RemoveMetadataCommandHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveMetadataCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var ok = _applicationService.RemoveMetadata(command);
            return ExecutionResult<bool>.Success(ok);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<bool>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
