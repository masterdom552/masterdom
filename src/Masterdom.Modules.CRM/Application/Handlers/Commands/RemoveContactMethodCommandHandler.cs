using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles remove-contact-method command orchestration.
/// </summary>
public sealed class RemoveContactMethodCommandHandler : ICommandHandler<RemoveContactMethodCommand, ExecutionResult<bool>>
{
    private readonly IPartyApplicationService _applicationService;

    public RemoveContactMethodCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveContactMethodCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var removed = _applicationService.RemoveContactMethod(command);
            return removed
                ? ExecutionResult<bool>.Success(true)
                : ExecutionResult<bool>.Failure("not_found", "The contact method was not found.");
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<bool>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
