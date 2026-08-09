using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Queries;

/// <summary>
/// Handles search for parties by effective role.
/// </summary>
public sealed class SearchPartiesByRoleQueryHandler : IQueryHandler<SearchPartiesByRoleQuery, ExecutionResult<IReadOnlyCollection<Party>>>
{
    private readonly IPartyApplicationService _applicationService;

    public SearchPartiesByRoleQueryHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<Party>> Handle(SearchPartiesByRoleQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var parties = _applicationService.SearchPartiesByRole(query);
        return ExecutionResult<IReadOnlyCollection<Party>>.Success(parties);
    }
}
