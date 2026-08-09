using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Queries;

/// <summary>
/// Handles search-ready party retrieval queries.
/// </summary>
public sealed class SearchPartiesQueryHandler : IQueryHandler<SearchPartiesQuery, ExecutionResult<IReadOnlyCollection<Party>>>
{
    private readonly IPartyApplicationService _applicationService;

    public SearchPartiesQueryHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<Party>> Handle(SearchPartiesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var parties = _applicationService.SearchParties(query);
        return ExecutionResult<IReadOnlyCollection<Party>>.Success(parties);
    }
}
