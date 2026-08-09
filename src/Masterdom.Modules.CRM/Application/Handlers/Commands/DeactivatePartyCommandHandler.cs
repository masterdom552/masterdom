using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles party-deactivation command orchestration.
/// </summary>
public sealed class DeactivatePartyCommandHandler : ICommandHandler<DeactivatePartyCommand, ExecutionResult<Party>>
{
    private readonly IPartyApplicationService _applicationService;

    public DeactivatePartyCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Party> Handle(DeactivatePartyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var party = _applicationService.DeactivateParty(command);
            return ExecutionResult<Party>.Success(party);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Party>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
