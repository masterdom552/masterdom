using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles party-update command orchestration.
/// </summary>
public sealed class UpdatePartyCommandHandler : ICommandHandler<UpdatePartyCommand, ExecutionResult<Party>>
{
    private readonly IPartyApplicationService _applicationService;

    public UpdatePartyCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Party> Handle(UpdatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var party = _applicationService.UpdateParty(command);
            return ExecutionResult<Party>.Success(party);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Party>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Party>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
