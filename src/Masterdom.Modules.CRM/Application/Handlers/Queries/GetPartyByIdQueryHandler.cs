using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Handlers.Queries;

/// <summary>
/// Handles party retrieval by identifier.
/// </summary>
public sealed class GetPartyByIdQueryHandler : IQueryHandler<GetPartyByIdQuery, ExecutionResult<Party>>
{
    private readonly IPartyApplicationService _applicationService;

    public GetPartyByIdQueryHandler(IPartyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Party> Handle(GetPartyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var party = _applicationService.GetParty(query);
        return party is null
            ? ExecutionResult<Party>.Failure("not_found", $"Party '{query.PartyId}' was not found.")
            : ExecutionResult<Party>.Success(party);
    }
}
