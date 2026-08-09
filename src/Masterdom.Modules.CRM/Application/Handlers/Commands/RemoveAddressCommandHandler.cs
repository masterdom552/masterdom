using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles remove-address command orchestration.
/// </summary>
public sealed class RemoveAddressCommandHandler : ICommandHandler<RemoveAddressCommand, ExecutionResult<bool>>
{
    private readonly IPartyApplicationService _applicationService;

    public RemoveAddressCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveAddressCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var removed = _applicationService.RemoveAddress(command);
            return removed
                ? ExecutionResult<bool>.Success(true)
                : ExecutionResult<bool>.Failure("not_found", "The address was not found.");
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
