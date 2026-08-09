using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Commands;

/// <summary>
/// Handles create-relationship command orchestration.
/// </summary>
public sealed class CreateRelationshipCommandHandler : ICommandHandler<CreateRelationshipCommand, ExecutionResult<Party>>
{
    private readonly IPartyApplicationService _applicationService;

    public CreateRelationshipCommandHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Party> Handle(CreateRelationshipCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var party = _applicationService.CreateRelationship(command);
            return ExecutionResult<Party>.Success(party);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Party>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
