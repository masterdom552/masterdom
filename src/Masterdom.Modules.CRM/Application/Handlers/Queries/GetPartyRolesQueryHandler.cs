using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Queries;

/// <summary>
/// Handles retrieval of party role history.
/// </summary>
public sealed class GetPartyRolesQueryHandler : IQueryHandler<GetPartyRolesQuery, ExecutionResult<IReadOnlyCollection<PartyRoleAssignment>>>
{
    private readonly IPartyApplicationService _applicationService;

    public GetPartyRolesQueryHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<PartyRoleAssignment>> Handle(GetPartyRolesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var roles = _applicationService.GetPartyRoles(query);
        return ExecutionResult<IReadOnlyCollection<PartyRoleAssignment>>.Success(roles);
    }
}
