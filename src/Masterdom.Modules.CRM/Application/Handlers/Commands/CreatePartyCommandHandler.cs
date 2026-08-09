using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles party-creation command orchestration.
/// </summary>
public sealed class CreatePartyCommandHandler : ICommandHandler<CreatePartyCommand, ExecutionResult<Party>>
{
    private readonly IPartyApplicationService _applicationService;

    public CreatePartyCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Party> Handle(CreatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var party = _applicationService.CreateParty(command);
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
